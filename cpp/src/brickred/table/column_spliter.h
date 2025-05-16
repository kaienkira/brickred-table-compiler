#ifndef BRICKRED_TABLE_COLUMN_SPLITER_H
#define BRICKRED_TABLE_COLUMN_SPLITER_H

#include <cstdint>
#include <string>

namespace brickred::table {

class ColumnSpliter final {
public:
    // only save the reference of the string
    // be attention with string lifetime
    ColumnSpliter(const std::string &text, char delimiter);
    ~ColumnSpliter();

    bool nextInt(int32_t *value);
    bool nextString(std::string *value);

private:
    const std::string &text_;
    char delimiter_;
    size_t read_index_;
};

} // namespace brickred::table

#endif
