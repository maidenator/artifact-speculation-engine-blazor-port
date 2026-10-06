#pragma once
#include <string>
#include <vector>
#include <cstdint>
#include <algorithm>

#include "simulation_types.hpp"
#include "serializer.hpp"
#include "values.hpp"
#include "../artifact/types.hpp"
#include "../artifact/random_utils.hpp"
#include "../artifact/generator.hpp"

class ArtifactInterface {
public:
    ArtifactInterface() : masterRng(1337) {}
    explicit ArtifactInterface(uint64_t seed) : masterRng(seed) {}

    void setSeed(uint64_t seed) {
        masterRng = rng::Xoshiro256(seed);
    }

    std::string generateBatchJson(int count, bool upgrade) {
        if (count <= 0) {
            return "[]";
        }
        std::vector<Artifact> batch;
        batch.reserve(static_cast<size_t>(count));
        for (int i = 0; i < count; ++i) {
            Artifact art = generator::generateArtifact(masterRng, false);
            if(upgrade) {
                for (int step = 0; step < 5; ++step) {
                    generator::upgradeArtifactOnce(art, masterRng);
                }
            }
            batch.push_back(art);
        }
        return serializer::serializeArtifactsJson(batch);
    }

    std::string generateBatchWithHistoryJson(int count) {
        if (count <= 0) {
            return "[]";
        }
        std::vector<std::vector<Artifact>> batches;
        batches.reserve(static_cast<size_t>(count));
        for (int i = 0; i < count; ++i) {
            std::vector<Artifact> history;
            history.reserve(6);

            // Base artifact (+0)
            Artifact art = generator::generateArtifact(masterRng, false);
            history.push_back(art);
            
            // Upgrade 5 times to +20, saving state each time
            for (int step = 0; step < 5; ++step) {
                generator::upgradeArtifactOnce(art, masterRng);
                history.push_back(art);
            }
            batches.push_back(history);
        }
        return serializer::serializeArtifactHistoriesJson(batches);
    }

    std::string runSimulationJson(const std::string &configJson) {
        SimulationConfig config = serializer::parseConfigJson(configJson);
        SimulationSummary summary = executeSimulation(config);
        return serializer::serializeSummaryJson(summary);
    }

    SimulationSummary executeSimulation(const SimulationConfig &config);

private:
    rng::Xoshiro256 masterRng;
};

inline SimulationSummary ArtifactInterface::executeSimulation(const SimulationConfig &config) {
    SimulationSummary summary;

    const size_t effectiveK = std::clamp(static_cast<size_t>(config.topK), size_t(1), size_t(20));

    struct ScoredArtifact {
        Artifact art;
        double score;
    };
    std::vector<ScoredArtifact> reservoir;
    reservoir.reserve(effectiveK + 1);

    auto updateReservoir = [&](const Artifact &art, double score) {
        // Common case: full and not good enough — no copy, no sort.
        if (reservoir.size() == effectiveK && score <= reservoir.back().score) return;
        auto it = std::upper_bound(reservoir.begin(), reservoir.end(), score,
            [](double s, const ScoredArtifact &e) { return s > e.score; });
        reservoir.insert(it, {art, score});
        if (reservoir.size() > effectiveK) {
            reservoir.pop_back();
        }
    };

    int junkFiveStarCount = 0;

    auto processPiece = [&](Artifact &art) -> bool {
        if (config.targetSlot.has_value() && art.slot != *config.targetSlot) {
            junkFiveStarCount++;
            return false;
        }
        if (config.targetMainStat.has_value() && art.mainStat.type != *config.targetMainStat) {
            junkFiveStarCount++;
            return false;
        }

        // Upgrade piece to Level 20
        for (int step = 0; step < 5; ++step) {
            generator::upgradeArtifactOnce(art, masterRng);
        }

        double score = evaluateArtifactScore(art, config);
        updateReservoir(art, score);

        bool metCriteria = satisfiesTargetCriteria(art, config, score);
        if (!metCriteria) {
            junkFiveStarCount++;
        }
        return metCriteria;
    };

    while (true) {
        if (config.mode == SimulationMode::FixedResin && summary.totalResinSpent >= config.resinBudget) {
            break;
        }
        if (config.mode == SimulationMode::TargetGoal) {
            if (summary.targetAchieved) {
                break;
            }
            // A non-positive budget with an unreached goal can never terminate.
            if (config.resinBudget <= 0) {
                break;
            }
            if (summary.totalResinSpent >= config.resinBudget) {
                break;
            }
        }

        summary.totalResinSpent += distributions::RESIN_PER_RUN;
        summary.domainRunsCompleted++;

        int dropsThisRun = (rng::fastUniformDouble(masterRng) < distributions::DOUBLE_5_STAR_CHANCE) ? 2 : 1;

        for (int d = 0; d < dropsThisRun; ++d) {
            summary.totalFiveStarsFound++;

            if (rng::fastUniformDouble(masterRng) < distributions::SET_SPLIT_RATE) {
                Artifact art = generator::generateArtifact(masterRng, false);
                if (processPiece(art) && config.mode == SimulationMode::TargetGoal) {
                    summary.targetAchieved = true;
                    break;
                }
            } else {
                junkFiveStarCount++;
            }
        }

        // No post-win strongbox processing: the goal was already met.
        if (summary.targetAchieved) {
            break;
        }

        if (config.useStrongBox) {
            while (junkFiveStarCount >= 3) {
                junkFiveStarCount -= 3;
                summary.strongboxRollsCompleted++;
                summary.totalFiveStarsFound++;

                Artifact boxArt = generator::generateArtifact(masterRng, false);
                if (processPiece(boxArt) && config.mode == SimulationMode::TargetGoal) {
                    summary.targetAchieved = true;
                    break;
                }
            }
        }
    }

    summary.equivalentDays = values::resinToDaysExact(summary.totalResinSpent);
    summary.topArtifacts.reserve(reservoir.size());
    for (const auto &entry : reservoir) {
        summary.topArtifacts.push_back(entry.art);
    }

    return summary;
}