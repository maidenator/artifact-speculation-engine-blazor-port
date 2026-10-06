#pragma once
#include <cstdint>

namespace values
{
    constexpr int RESIN_PER_RUN = 20;
    constexpr double RESIN_PER_DAY = 180.0;

    inline int daysToResin(int days) {
        return days * static_cast<int>(RESIN_PER_DAY);
    }

    inline int resinToDays(int resin) {
        return resin / static_cast<int>(RESIN_PER_DAY);
    }

    inline int resinToDomainRuns(int resin) {
        return resin / RESIN_PER_RUN;
    }

    inline double resinToDaysExact(int64_t resin) {
        return static_cast<double>(resin) / RESIN_PER_DAY;
    }
}