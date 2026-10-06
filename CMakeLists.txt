cmake_minimum_required(VERSION 3.14)
project(artifact-speculation-engine CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include(FetchContent)

# --- Dependencies ---
FetchContent_Declare(
    Catch2
    GIT_REPOSITORY https://github.com/catchorg/Catch2.git
    GIT_TAG        fc94246664433dbdec2f766b939bb075cfcb8c23 # v3.5.2
)

FetchContent_Declare(
    nlohmann_json
    URL https://github.com/nlohmann/json/releases/download/v3.11.3/json.tar.xz
    URL_HASH SHA256=d6c65aca6b1ed68e7a182f4757257b107ae403032760ed6ef121c9d55e81757d
)

FetchContent_MakeAvailable(Catch2 nlohmann_json)

# --- Test Target ---
add_executable(tests
    cpp/tests/test_distributions.cpp
    cpp/tests/test_random_utils.cpp
    cpp/tests/test_generator.cpp
    cpp/tests/test_simulator.cpp
    cpp/tests/test_artifact_interface.cpp
    cpp/tests/test_serializer.cpp
    cpp/tests/test_simulation_types.cpp
    cpp/tests/test_config_validation.cpp
)

target_include_directories(tests PRIVATE 
    cpp
    cpp/artifact
    cpp/domain
)

target_link_libraries(tests PRIVATE 
    Catch2::Catch2WithMain 
    nlohmann_json::nlohmann_json
)

# Review §7: warnings on for project code (nlohmann headers stay quiet
# because they come through the FetchContent interface target).
if (CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang|Emscripten")
    target_compile_options(tests PRIVATE -Wall -Wextra -Wconversion -Werror)
endif()

# --- Test Discovery ---
include(CTest)
include(Catch)
catch_discover_tests(tests)