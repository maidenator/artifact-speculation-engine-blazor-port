#include <catch2/catch_test_macros.hpp>
#include <stdexcept>
#include <nlohmann/json.hpp>

#include "domain/serializer.hpp"
#include "domain/artifact_interface.hpp"

// =====================================================================
// Engine-boundary validation (review §3: unchecked enum casts, unbounded
// resinBudget, TargetGoal infinite loop on non-positive budget)
// =====================================================================

TEST_CASE("parseConfigJson rejects out-of-range enums") {
    REQUIRE_THROWS_AS(serializer::parseConfigJson(R"({"mode": 99})"), std::invalid_argument);
    REQUIRE_THROWS_AS(serializer::parseConfigJson(R"({"mode": -1})"), std::invalid_argument);
    REQUIRE_THROWS_AS(serializer::parseConfigJson(R"({"targetSlot": 9})"), std::invalid_argument);
    REQUIRE_THROWS_AS(serializer::parseConfigJson(R"({"targetMainStat": 42})"), std::invalid_argument);
    REQUIRE_THROWS_AS(
        serializer::parseConfigJson(R"({"substatWeights": [{"stat": 15, "weight": 1.0}]})"),
        std::invalid_argument);
}

TEST_CASE("parseConfigJson accepts boundary enum values") {
    auto config = serializer::parseConfigJson(R"({
        "mode": 1,
        "targetSlot": 4,
        "targetMainStat": 18,
        "substatWeights": [{"stat": 9, "weight": 0.5}]
    })");
    REQUIRE(config.mode == SimulationMode::TargetGoal);
    REQUIRE(config.targetSlot == ArtifactSlot::circlet);
    REQUIRE(config.targetMainStat == ArtifactMainStat::physicalDmg);
    REQUIRE(config.substatWeights.size() == 1);
}

TEST_CASE("parseConfigJson clamps resinBudget into [20, 10000000]") {
    REQUIRE(serializer::parseConfigJson(R"({"resinBudget": -50})").resinBudget == 20);
    REQUIRE(serializer::parseConfigJson(R"({"resinBudget": 0})").resinBudget == 20);
    REQUIRE(serializer::parseConfigJson(R"({"resinBudget": 2000})").resinBudget == 2000);
    REQUIRE(serializer::parseConfigJson(R"({"resinBudget": 999999999})").resinBudget == 10000000);
}

TEST_CASE("TargetGoal with non-positive budget terminates immediately") {
    ArtifactInterface engine(7);

    SimulationConfig config;
    config.mode = SimulationMode::TargetGoal;
    config.targetSlot = ArtifactSlot::flower;
    config.minCritValue = 20.0;
    config.resinBudget = 0;

    auto summary = engine.executeSimulation(config);

    REQUIRE_FALSE(summary.targetAchieved);
    REQUIRE(summary.totalResinSpent == 0);
    REQUIRE(summary.domainRunsCompleted == 0);
}

TEST_CASE("generateBatchJson rejects non-positive counts") {
    ArtifactInterface engine(7);

    REQUIRE(nlohmann::json::parse(engine.generateBatchJson(0, true)).empty());
    REQUIRE(nlohmann::json::parse(engine.generateBatchJson(-5, false)).empty());
}

TEST_CASE("getMainStatWeights rejects invalid slots") {
    REQUIRE_THROWS_AS(
        distributions::getMainStatWeights(static_cast<ArtifactSlot>(9)),
        std::invalid_argument);
}
