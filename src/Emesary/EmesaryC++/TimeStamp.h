#pragma once

#include <iosfwd>
#include <iomanip>
#include <sstream>
#include <Windows.h>

// based on simgear::timestamp.hxx by Curtis Olson, December 1998.

namespace Emesary
{

#ifdef _WIN32
    static bool qpc_init = false;
    static LARGE_INTEGER s_frequency;
    static BOOL s_use_qpc;
#endif

    class TimeStamp {
    public:
        typedef long sec_type;
        typedef int nsec_type;

        TimeStamp() :
            _nsec(0),
            _sec(0)
        {
            stamp();
        }

        void stamp() {
            if (!qpc_init) {
                s_use_qpc = QueryPerformanceFrequency(&s_frequency);
                qpc_init = true;
            }
            if (qpc_init && s_use_qpc) {
                LARGE_INTEGER now;
                QueryPerformanceCounter(&now);
                _sec = static_cast<long>(now.QuadPart / s_frequency.QuadPart);
                _nsec = static_cast<long>((1000000000LL * (now.QuadPart - _sec * s_frequency.QuadPart)) / s_frequency.QuadPart);
            }
			else { throw "Unable to stamp()"; }
            /*else {
                unsigned int t;

                t = timeGetTime();
                _sec = t / 1000;
                _nsec = (t - (_sec * 1000)) * 1000 * 1000;
            }*/
        }


        /** @return the saved seconds of this time stamp */
        long get_seconds() const { return _sec; }

        /** @return the saved microseconds of this time stamp */
        int get_usec() const { return _nsec / 1000; }

        /** @return the saved seconds of this time stamp */
        const sec_type& getSeconds() const
        {
            return _sec;
        }
        /** @return the saved nanoseconds of this time stamp */
        const nsec_type& getNanoSeconds() const
        {
            return _nsec;
        }

        /** @return the value of the timestamp in nanoseconds,
         *  use doubles to avoid overflow.
         *  If you need real nanosecond accuracy for time differences, build up a
         *  TimeStamp reference time and compare TimeStamps directly.
         */
        double toNSecs() const
        {
            return _nsec + double(_sec) * 1000 * 1000 * 1000;
        }

        /** @return the value of the timestamp in microseconds,
         *  use doubles to avoid overflow.
         *  If you need real nanosecond accuracy for time differences, build up a
         *  TimeStamp reference time and compare TimeStamps directly.
         */
        double toUSecs() const
        {
            return 1e-3 * _nsec + double(_sec) * 1000 * 1000;
        }

        /** @return the value of the timestamp in milliseconds,
         *  use doubles to avoid overflow.
         *  If you need real nanosecond accuracy for time differences, build up a
         *  TimeStamp reference time and compare TimeStamps directly.
         */
        double toMSecs() const
        {
            return 1e-6 * _nsec + double(_sec) * 1000;
        }

        double toSecs() const
        {
            return 1e-9 * _nsec + _sec;
        }

        /**

        /**
         *  Return a timestamp with the current time.
         */
        static TimeStamp now()
        {
            TimeStamp ts; ts.stamp(); return ts;
        }

        TimeStamp& operator+=(const TimeStamp& c)
        {
            _sec += c._sec;
            _nsec += c._nsec;
            if ((1000 * 1000 * 1000) <= _nsec) {
                _nsec -= (1000 * 1000 * 1000);
                _sec += 1;
            }
            return *this;
        }

        TimeStamp& operator-=(const TimeStamp& c)
        {
            _sec -= c._sec;
            _nsec -= c._nsec;
            if (_nsec < 0) {
                _nsec += (1000 * 1000 * 1000);
                _sec -= 1;
            }
            return *this;
        }
        TimeStamp operator+(const TimeStamp& c1)
        {
            return c1;
        }

        TimeStamp  operator - (const TimeStamp& c)
        {
            _sec -= c._sec;
            _nsec -= c._nsec;
            if (_nsec < 0) {
                _nsec += (1000 * 1000 * 1000);
                _sec -= 1;
            }
            return *this;
        }

        /**
        * elapsed time since the stamp was taken, in msec
        */
        int elapsedMSec() const {
            TimeStamp now;
            now.stamp();

            return static_cast<int>((now - *this).toMSecs());
        }
        /**
        * elapsed time since the stamp was taken, in usec
        */
        int elapsedUSec() const {
            TimeStamp now;
            now.stamp();

            return static_cast<int>((now - *this).toUSecs());
        }

        /// output rate based on total number of operations expressed in millions / sec
        void output_rate_per_Msec(const unsigned int& total_operations)
        {
            auto rate_perUs = elapsedUSec() / static_cast<double>(total_operations);
            auto per_second = (static_cast<unsigned>(round(1000000 / rate_perUs))) / 1000000.0;
            std::cout << "elapsed " << elapsedUSec() / 1000000.0 << ", average " << rate_perUs << " uS per operation, rate " << std::fixed << per_second << "/M/sec" << std::endl;
        }

    private:
        nsec_type _nsec;
        sec_type _sec;
    };

    //inline bool
    //operator==(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    if (c1.getNanoSeconds() != c2.getNanoSeconds())
    //        return false;
    //    return c1.getSeconds() == c2.getSeconds();
    //}
    //
    //inline bool
    //operator!=(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    return !operator==(c1, c2);
    //}
    //
    //inline bool
    //operator<(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    if (c1.getSeconds() < c2.getSeconds())
    //        return true;
    //    if (c1.getSeconds() > c2.getSeconds())
    //        return false;
    //    return c1.getNanoSeconds() < c2.getNanoSeconds();
    //}
    //
    //inline bool
    //operator>(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    return c2 < c1;
    //}
    //
    //inline bool
    //operator>=(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    return !(c1 < c2);
    //}
    //
    //inline bool
    //operator<=(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    return !(c1 > c2);
    //}
    //
    //inline TimeStamp
    //operator+(const TimeStamp& c1)
    //{
    //    return c1;
    //}
    //
    //inline TimeStamp
    //operator-(const TimeStamp& c1)
    //{
    //    return TimeStamp::fromSec(0) -= c1;
    //}
    //
    //inline TimeStamp
    //operator+(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    return TimeStamp(c1) += c2;
    //}
    //
    //inline TimeStamp
    //operator-(const TimeStamp& c1, const TimeStamp& c2)
    //{
    //    return TimeStamp(c1) -= c2;
    //}

    template<typename char_type, typename traits_type>
    inline
        std::basic_ostream<char_type, traits_type>&
        operator<<(std::basic_ostream<char_type, traits_type>& os, const TimeStamp& c)
    {
        std::basic_stringstream<char_type, traits_type> stream;

        TimeStamp pos = c;
        if (c.getSeconds() < 0) {
            stream << stream.widen('-');
            pos = TimeStamp() - c;
        }
        stream << pos.getSeconds() << stream.widen('.');
        stream << std::setw(9) << std::setfill('0') << pos.getNanoSeconds();

        return os << stream.str();
    }
}