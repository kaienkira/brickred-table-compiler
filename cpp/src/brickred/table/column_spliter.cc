#include <brickred/table/column_spliter.h>

#include <cstdlib>

namespace brickred::table {

ColumnSpliter::ColumnSpliter(const std::string &text, char delimiter) :
    text_(text), delimiter_(delimiter), read_index_(0)
{
}

ColumnSpliter::~ColumnSpliter()
{
}

bool ColumnSpliter::nextInt(int32_t *value)
{
    if (value != nullptr) {
        std::string ret;
        if (nextString(&ret) == false) {
            return false;
        }
        *value = ::atoi(ret.c_str());
    } else {
        if (nextString(nullptr) == false) {
            return false;
        }
    }

    return true;
}

bool ColumnSpliter::nextString(std::string *value)
{
    if (read_index_ > text_.size()) {
        return false;
    } else if (read_index_ == text_.size()) {
        if (value != nullptr) {
            *value = "";
        }
        read_index_ += 1;
        return true;
    }

    for (size_t i = read_index_; i < text_.size(); ++i) {
        char c = text_[i];

        if (c == delimiter_) {
            if (value != nullptr) {
                *value = std::string(&text_[read_index_],
                    i - read_index_);
            }
            read_index_ = i + 1;
            return true;
        }
    }

    if (read_index_ < text_.size()) {
        if (value != nullptr) {
            *value = std::string(&text_[read_index_],
                text_.size() - read_index_);
        }
        read_index_ = text_.size() + 1;
        return true;
    }

    return false;
}

} // namespace brickred::table
