#pragma once
#include <string>
#include <vector>
#include <algorithm>
#include <stdexcept>
#include <nlohmann/json.hpp>
#include "simulation_types.hpp"
#include "../artifact/generator.hpp"


namespace serializer {

constexpr int MIN_RESIN_BUDGET = 20;
constexpr int MAX_RESIN_BUDGET = 10000000;

inline SimulationMode checkedMode(int v) {
    if (v < 0 || v > 1) throw std::invalid_argument("mode out of range (0-1)");
    return static_cast<SimulationMode>(v);
}

inline ArtifactSlot checkedSlot(int v) {
    if (v < 0 || v > 4) throw std::invalid_argument("targetSlot out of range (0-4)");
    return static_cast<ArtifactSlot>(v);
}

inline ArtifactMainStat checkedMainStat(int v) {
    if (v < 0 || v > 18) throw std::invalid_argument("targetMainStat out of range (0-18)");
    return static_cast<ArtifactMainStat>(v);
}

inline ArtifactSubstat checkedSubstat(int v) {
    if (v < 0 || v > 9) throw std::invalid_argument("substat out of range (0-9)");
    return static_cast<ArtifactSubstat>(v);
}

inline SimulationConfig parseConfigJson(const std::string &jsonStr) {
    using json = nlohmann::json;
    json j = json::parse(jsonStr);
    SimulationConfig config;

    if (j.contains("mode")) config.mode = checkedMode(j["mode"].get<int>());

    if (j.contains("targetSlot") && !j["targetSlot"].is_null()) {
        config.targetSlot = checkedSlot(j["targetSlot"].get<int>());
    }

    if (j.contains("targetMainStat") && !j["targetMainStat"].is_null()) {
        config.targetMainStat = checkedMainStat(j["targetMainStat"].get<int>());
    }

    if (j.contains("resinBudget")) {
        config.resinBudget = std::clamp(j["resinBudget"].get<int>(), MIN_RESIN_BUDGET, MAX_RESIN_BUDGET);
    }
    if (j.contains("topK")) config.topK = j["topK"].get<int>();
    if (j.contains("useStrongBox")) config.useStrongBox = j["useStrongBox"].get<bool>();
    if (j.contains("minCritValue")) config.minCritValue = j["minCritValue"].get<double>();

    if (j.contains("substatWeights")) {
        for (const auto &w : j["substatWeights"]) {
            config.substatWeights.push_back({
                checkedSubstat(w["stat"].get<int>()),
                w["weight"].get<double>()
            });
        }
    }

    return config;
}

inline nlohmann::json artifactToJson(const Artifact &art) {
    using json = nlohmann::json;
    json a;
    a["slot"] = static_cast<int>(art.slot);
    a["level"] = art.level;

    a["mainStat"] = {
        {"type", static_cast<int>(art.mainStat.type)},
        {"value", art.mainStat.value}
    };

    json subs = json::array();
    for (size_t i = 0; i < static_cast<size_t>(art.substatCount); ++i) {
        subs.push_back({
            {"type", static_cast<int>(art.subStats[i].type)},
            {"value", art.subStats[i].value},
            {"rolls", art.subStats[i].rolls}
        });
    }
    a["subStats"] = subs;
    a["critValue"] = generator::calculateCritValue(art);

    return a;
}

inline std::string serializeSummaryJson(const SimulationSummary &summary) {
    using json = nlohmann::json;
    json j;

    j["targetAchieved"] = summary.targetAchieved;
    j["totalResinSpent"] = summary.totalResinSpent;
    j["equivalentDays"] = summary.equivalentDays;
    j["domainRunsCompleted"] = summary.domainRunsCompleted;
    j["strongboxRollsCompleted"] = summary.strongboxRollsCompleted;
    j["totalFiveStarsFound"] = summary.totalFiveStarsFound;

    json topArts = json::array();
    for (const auto &art : summary.topArtifacts) {
        topArts.push_back(artifactToJson(art));
    }
    j["topArtifacts"] = topArts;

    return j.dump();
}

inline std::string serializeArtifactsJson(const std::vector<Artifact> &artifacts) {
    using json = nlohmann::json;
    json arr = json::array();

    for (const auto &art : artifacts) {
        arr.push_back(artifactToJson(art));
    }

    return arr.dump();
}

inline std::string serializeArtifactHistoriesJson(const std::vector<std::vector<Artifact>> &histories) {
    using json = nlohmann::json;
    json arr = json::array();

    for (const auto &history : histories) {
        json histArr = json::array();
        for (const auto &art : history) {
            histArr.push_back(artifactToJson(art));
        }
        arr.push_back(histArr);
    }

    return arr.dump();
}

} // namespace serializer