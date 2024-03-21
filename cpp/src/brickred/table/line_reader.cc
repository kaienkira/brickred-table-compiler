#include <brickred/table/line_reader.h>

namespace brickred::table {

LineReader::LineReader(const std::string &text) :
    text_(text), read_index_(0)
{
}

LineReader::~LineReader()
{
}

const LineReader::LineBuffer *LineReader::nextLine()
{
    line_buffer_.clear();

    if (read_index_ >= text_.size()) {
        return nullptr;
    }

    Status::type status = Status::NORMAL;
    size_t col_start = read_index_;

    for (size_t i = read_index_; i < text_.size(); ++i) {
        char c = text_[i];

        if (status == Status::NORMAL) {
            if (c == '\t') {
                line_buffer_.push_back("");
                col_start = i + 1;
            } else if (c == '\r') {
                status = Status::READ_NEWLINE;
            } else {
                status = Status::READ_COLUMN;
            }
        } else if (status == Status::READ_COLUMN) {
            if (c == '\t') {
                line_buffer_.push_back(
                    getColumn(col_start, i));
                col_start = i + 1;
                status = Status::NORMAL;
            } else if (c == '\r') {
                status = Status::READ_NEWLINE;
            }
        } else if (status == Status::READ_NEWLINE) {
            if (c == '\n') {
                line_buffer_.push_back(
                    getColumn(col_start, i - 1));
                read_index_ = i + 1;
                return &line_buffer_;
            } else if (c == '\r') {
                continue;
            } else {
                status = Status::READ_COLUMN;
            }
        }
    }

    if (col_start < text_.size()) {
        line_buffer_.push_back(getColumn(col_start, text_.size()));
        read_index_ = text_.size();
        return &line_buffer_;
    }

    return nullptr;
}

std::string LineReader::getColumn(size_t col_start, size_t col_end)
{
    if (col_end - col_start >= 2 &&
        text_[col_start] == '"' &&
        text_[col_end - 1] == '"') {
        // trim quote mark
        col_start += 1;
        col_end -= 1;
        // convert double quote mark to single quote mark
        std::string str;
        str.reserve(col_end - col_start);
        size_t i = col_start;
        while (i < col_end - 1) {
            str.push_back(text_[i]);
            if (text_[i] == '"' &&
                text_[i + 1] == '"') {
                i += 2;
            } else {
                ++i;
            }
        }
        if (i == col_end - 1) {
            str.push_back(text_[i]);
        }
        return str;
    } else {
        return std::string(&text_[col_start], col_end - col_start);
    }
}

} // namespace brickred::table
