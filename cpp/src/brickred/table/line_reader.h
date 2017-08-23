#ifndef BRICKRED_TABLE_LINE_READER_H
#define BRICKRED_TABLE_LINE_READER_H

#include <cstddef>
#include <string>
#include <vector>

namespace brickred {
namespace table {

class LineReader {
public:
    typedef std::vector<std::string> LineBuffer;

    // only save the reference of the string
    // be attention with string lifetime
    LineReader(const std::string &text);
    ~LineReader();

    const LineBuffer *nextLine();

private:
    struct Status {
        enum type {
            NORMAL,
            READ_COLUMN,
            READ_NEWLINE,
        };
    };

    std::string getColumn(size_t col_start, size_t col_end);

private:
    const std::string &text_;
    size_t read_index_;
    LineBuffer line_buffer_;
};

} // namespace table
} // namespace brickred

#endif
