#ifndef BRICKRED_TABLE_UTIL_H
#define BRICKRED_TABLE_UTIL_H

#include <stdint.h>
#include <string>
#include <vector>

#include <brickred/table/column_spliter.h>

namespace brickred {
namespace table {
namespace util {

std::string error(const char *format, ...);

void readColumnIntList(
    const std::string &col, std::vector<int32_t> *ret);
void readColumnStringList(
    const std::string &col, std::vector<std::string> *ret);

template <class T>
bool readColumnStructList(const std::string &col, std::vector<T> *ret)
{
    if (col.empty()) {
        return true;
    }

    ColumnSpliter s(col, '|');
    std::string str;
    while (s.nextString(&str)) {
        T v;
        if (v.parse(str) == false) {
            return false;
        }
        ret->push_back(v);
    }

    return true;
}

} // namespace util
} // namespace table
} // namespace brickred

#endif
