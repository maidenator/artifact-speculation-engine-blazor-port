#pragma once
#include <cstdint>
#include <cassert>

namespace rng {
class Xoshiro256 {
    private:
        // The 256-bit state is stored as four random 64-bit integers
        uint64_t s[4];

        // Bitwise left rotation helper
        static inline uint64_t rotateLeft(const uint64_t x, int k) {
            return (x << k) | (x >> (64 - k));
        }

        static constexpr uint64_t GOLDEN_RATIO_CONST = 0x9e3779b97f4a7c15ULL;
        static constexpr uint64_t MIXER_MULTIPLIER_1 = 0xbf58476d1ce4e5b9ULL;
        static constexpr uint64_t MIXER_MULTIPLIER_2 = 0x94d049bb133111ebULL;

    public:
        explicit Xoshiro256(uint64_t seed) {
            /*
                Seed-expansion in the style of SplitMix64: mix a single seed
                into 4 healthy, non-zero state values. Note this is not the
                canonical SplitMix64 (which adds the golden-ratio constant
                between outputs); it only needs to decorrelate the 4 lanes.
            */
           uint64_t z = seed + GOLDEN_RATIO_CONST;
           for(int i = 0; i < 4; ++i) {
                z = (z ^ (z >> 30)) * MIXER_MULTIPLIER_1;
                z = (z ^ (z >> 27)) * MIXER_MULTIPLIER_2;

                s[i] = z ^ (z >> 31);
           } 
        }

        inline uint64_t operator() () {
            // 1. Generate the output value by mixing state elements
            const uint64_t result = rotateLeft(s[1]  * 5, 7) * 9;

            // 2. Shift and scramble the 256 bit vector
            const uint64_t t = s[1] << 17;

            s[2] ^= s[0];
            s[3] ^= s[1];
            s[1] ^= s[2];
            s[0] ^= s[3];

            s[2] ^= t;
            s[3] = rotateLeft(s[3], 45);

            return result;
        }
};

/**
 * Generates a random number between 0 and bound - 1
 * uses lemire's algorithm to avoid modulo bias
 * 
 * @param bound upper limit for the random number
 * @param rng the xoshiro256 random engine
 */
inline uint32_t fastUniform(int32_t bound, Xoshiro256 &rng) {
    assert(bound > 0 && "Bound must be a positive integer");

    uint64_t ubound = static_cast<uint64_t>(bound);
    uint64_t x = rng();

    // NOTE: __uint128_t is a GCC/Clang extension (fine for Emscripten and
    // MinGW builds here) and will not compile on MSVC.
    // Prevents overflow from the 64-bit rng output
    __uint128_t m = static_cast<__uint128_t>(x) * ubound;
    uint64_t l = static_cast<uint64_t>(m);

    // Correct Lemire rejection compares the full low 64 bits, not just the
    // low 32: reject while l falls in the biased zone [0, t).
    if (l < ubound) {
        const uint64_t t = (0 - ubound) % ubound;
        while (l < t) {
            x = rng();
            m = static_cast<__uint128_t>(x) * ubound;
            l = static_cast<uint64_t>(m);
        }
    }

    return static_cast<uint32_t>(m >> 64);
}

/**
 * Wraps fastUniform into a function that generates a random number between:
 * @param min lower bound for the random number
 * @param max upper bound for the random number
 * 
 * @param rng the xoshiro256 random engine
 */
inline int32_t fastUniformRange(int32_t min, int32_t max, Xoshiro256 &rng) {
    assert(min <= max && "Minimum must be less than or equal to maximum");

    int32_t range = (max - min) + 1;

    uint32_t randomOffset = fastUniform(range, rng);

    return min + static_cast<int32_t>(randomOffset);
}

/**
 * Uniform double in [0, 1), using the top 53 bits of one rng output.
 * For probability checks (e.g. `fastUniformDouble(rng) < CHANCE`).
 */
inline double fastUniformDouble(Xoshiro256 &rng) {
    return static_cast<double>(rng() >> 11) * (1.0 / 9007199254740992.0);
}
}