#pragma once
#include <cmath>
#include "types.hpp"
#include "random_utils.hpp"
#include "distributions.hpp"
namespace generator {

/**
 * Calculates the artifact's Crit Value (CV) using the standard formula:
 * Crit DMG + (Crit Rate * 2)
 * 
 * @param art The artifact object containing the substats to evaluate
 * @return double The total calculated crit value
 */
inline double calculateCritValue(const Artifact &art) {
    double critValue = 0.0;
    for(size_t i  = 0; i < static_cast<size_t>(art.substatCount); ++i) {
        const auto &sub = art.subStats[i];
        
        if(sub.type == ArtifactSubstat::critDmg)
            critValue += sub.value;
        if(sub.type == ArtifactSubstat::critRate)
            critValue += sub.value * 2.0;
    }
    return critValue;
}

/**
 * Selects a random main stat for a given artifact piece based on official drop weights.
 * 
 * @param pieceType The piece category (Flower, Plume, Sands, Goblet, Circlet).
 * @param rng Reference to the Xoshiro256 random engine.
 * @return ArtifactMainStat The selected main stat.
 */
inline MainStat generateMainStat(ArtifactSlot pieceType, rng::Xoshiro256 &rng) {
    // 1. Grab the weights for the given piece type
    auto weights = distributions::getMainStatWeights(pieceType);
    // 2. Generate a number between 0 - 100
    double roll = rng::fastUniformRange(0, 100, rng);

    // 3. Initialize a running sum that goes through the weight value/s of the given piece type.
    double sum = 0.0;
    ArtifactMainStat chosenStat = weights.back().stat; // Fallback default 

    for(const auto weight : weights) {
        sum += weight.weight;
        if(roll <= sum) {
            chosenStat = weight.stat;
            break;
        }
    }

    // 4. Automatically compute the Level 0 base value
    double baseValue = distributions::getMainStatValue(chosenStat, 0);

    // 5. Return the fully initialized MainStat struct (Type + Level 0 Value)
    return MainStat{ chosenStat, baseValue };
}

/**
 * Selects a random substat value given the substat.
 * 
 * @param subStat The subStat category (critDmg, critRate, etc.).
 * @param rng Reference to the Xoshiro256 random engine.
 * @return double The selected substat roll.
 */
inline double rollSubstatValue(ArtifactSubstat subStat, rng::Xoshiro256 &rng) {
    auto subStats = distributions::getSubstatValues(subStat);
    double rolledValue = subStats[static_cast<size_t>(rng::fastUniformRange(0, 3, rng))];
    return std::round(rolledValue * 10.0) / 10.0;
}

/**
 * Rolls the initial substats for a new artifact drop (handles 3 vs 4 starting lines and prevents duplicate stats).
 * 
 * @param mainStat The main stat type to ensure it doesn't duplicate into the substat pool.
 * @param rng Reference to the Xoshiro256 random engine.
 * @return Artifact The generated artifact with its initial rolls.
 */
inline Artifact generateArtifactSubstats(MainStat mainStat, rng::Xoshiro256 &rng) {
    Artifact art;

    // 1. Identify if we have a four liner (20% chance to start with four stats).
    bool isFourLiner = rng::fastUniformDouble(rng) < distributions::FOUR_LINER_CHANCE;

    // 2. Initialize a pool of 10 unselected Substats.
    // ALL_SUBSTATS is in ArtifactSubstat enum order, so index directly.
    std::array<bool, 10> selectedStats = {false};
    constexpr size_t substatSize = distributions::ALL_SUBSTATS.size();

    // 3. Mark mainStat as selected so we cannot pick it again.
    if (auto matchingSub = distributions::mainStatToSubStat(mainStat.type)) {
        selectedStats[static_cast<size_t>(*matchingSub)] = true;
    }

    // 4. Determine how many substats to roll
    int substatCount = isFourLiner? 4 : 3;
    art.substatCount = substatCount;

    // Calculate initial total weight
    uint32_t totalWeight = 0;
    for(size_t j = 0; j < substatSize; ++j) {
        if(!selectedStats[j]) totalWeight += distributions::getSubStatWeight(distributions::ALL_SUBSTATS[j]);
    }

    // 5. Loop to pick each substat
    for(int i = 0; i < substatCount; ++i) {
        // Roll random threshold
        uint32_t roll = rng::fastUniformRange(0, static_cast<int32_t>(totalWeight - 1), rng);
        uint32_t sum = 0;
        size_t chosenIndex = 0;

        // Find the winning substat
        for(size_t j = 0; j < substatSize; ++j) {
            if(selectedStats[j]) continue;

            sum += distributions::getSubStatWeight(distributions::ALL_SUBSTATS[j]);
            if(roll < sum) {
                chosenIndex = j;
                break;
            }
        }

        // Mark as selected so it won't repeat
        selectedStats[chosenIndex] = true;

        // Subtract the weight of the chosen stat from the total weight
        auto substatType = distributions::ALL_SUBSTATS[chosenIndex];
        totalWeight -= distributions::getSubStatWeight(substatType);

        art.subStats[i] = {
            substatType,
            rollSubstatValue(substatType, rng),
            1
        };
    }

    return art;
}

/**
 * Generates a unique artifact ID, 
 * 
 * @param rng Reference to the Xoshiro256 random engine.
 * @return String The generated id with the format "art_[id]".
 */
inline std::string generateArtifactId(rng::Xoshiro256 &rng) {
    return "art_" + std::to_string(rng::fastUniformRange(100000000, 999999999, rng));
}

/**
 * Generates a complete random 5-star Genshin Impact artifact,
 * including slot, main stat, and randomized substats.
 * IDs cost an RNG draw plus a heap string each; only request one when the
 * caller actually keeps it (batch paths), never in the hot simulation loop.
 *
 * @param rng Reference to the Xoshiro256 random engine.
 * @param assignId Whether to generate the "art_[id]" identifier.
 * @return Artifact The fully generated artifact object.
 */
inline Artifact generateArtifact(rng::Xoshiro256 &rng, bool assignId = true) {
    Artifact art;

    if (assignId) {
        art.id = generateArtifactId(rng);
    }
    art.slot = static_cast<ArtifactSlot>(rng::fastUniformRange(0, 4, rng));
    art.level = 0;
    
    art.mainStat = generateMainStat(art.slot, rng);
    
    Artifact substatPackage = generateArtifactSubstats(art.mainStat, rng);
    
    art.substatCount = substatPackage.substatCount;
    art.subStats = substatPackage.subStats;

    return art;
}

/**
 * Simulates upgrading an artifact level, either adding a new substat or boosting an existing one.
 * 
 * @param art Reference to the artifact being upgraded.
 * @param rng Reference to the Xoshiro256 random engine.
 */
inline void upgradeArtifactOnce(Artifact &art, rng::Xoshiro256 &rng) {
    assert(art.level < 20 && "Artifact level must be less than 20");

    constexpr size_t totalPoolSize = distributions::ALL_SUBSTATS.size();

    // Initialize a pool of 10 unselected Substats
    std::array<bool, 10> selectedStats = {false};

    // Mark the Main Stat as selected
    if (auto matchingSub = distributions::mainStatToSubStat(art.mainStat.type)) {
        selectedStats[static_cast<size_t>(*matchingSub)] = true;
    }

    // Mark all *currently existing* substats as selected
    for (size_t i = 0; i < static_cast<size_t>(art.substatCount); ++i) {
        selectedStats[static_cast<size_t>(art.subStats[i].type)] = true;
    }

    // Check if the artifact is a four liner
    bool isFourLiner = (art.substatCount == 4);

    if (!isFourLiner) {
        // Case A: Adding the 4th substat (3-liner upgrades to 4-liner)
        uint32_t totalWeight = 0;
        for (size_t j = 0; j < totalPoolSize; ++j) {
            if (!selectedStats[j]) {
                totalWeight += distributions::getSubStatWeight(distributions::ALL_SUBSTATS[j]);
            }
        }

        uint32_t roll = rng::fastUniformRange(0, static_cast<int32_t>(totalWeight - 1), rng);
        uint32_t sum = 0;
        size_t chosenIndex = 0;

        for (size_t j = 0; j < totalPoolSize; ++j) {
            if (selectedStats[j]) continue;
            sum += distributions::getSubStatWeight(distributions::ALL_SUBSTATS[j]);
            if (roll < sum) {
                chosenIndex = j;
                break;
            }
        }

        // Add the new 4th substat with rolls = 1
        auto substatType = distributions::ALL_SUBSTATS[chosenIndex];
        art.subStats[3] = {
            substatType,
            rollSubstatValue(substatType, rng),
            1
        };
        art.substatCount = 4;
    } else {
        // Case B: Boosting an existing substat (Randomly pick one of the 4 to upgrade)
        size_t upgradeIndex = static_cast<size_t>(rng::fastUniformRange(0, 3, rng));
        art.subStats[upgradeIndex].value += rollSubstatValue(art.subStats[upgradeIndex].type, rng);
        art.subStats[upgradeIndex].rolls++;
    }

    art.level += 4;
    art.mainStat.value = distributions::getMainStatValue(art.mainStat.type, art.level);
}
} // namespace generator