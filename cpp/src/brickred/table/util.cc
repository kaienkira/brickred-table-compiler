#include <brickred/table/util.h>

#include <cstdarg>
#include <cstdio>

namespace brickred::table::util {

std::string error(const char *format, ...)
{
    char buffer[1024];

    va_list args;
    va_start(args, format);
    ::vsnprintf(buffer, sizeof(buffer), format, args);
    va_end(args);

    return std::string(buffer);
}

void readColumnIntList(
    const std::string &col, std::vector<int32_t> *ret)
{
    if (col.empty()) {
        return;
    }

    ColumnSpliter s(col, '|');
    int32_t v;
    while (s.nextInt(&v)) {
        ret->push_back(v);
    }
}

void readColumnStringList(
    const std::string &col, std::vector<std::string> *ret)
{
    if (col.empty()) {
        return;
    }

    ColumnSpliter s(col, '|');
    std::string v;
    while (s.nextString(&v)) {
        ret->push_back(v);
    }
}

} // namespace brickred::table::util
