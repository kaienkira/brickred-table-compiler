#include <iconv.h>
#include <cstddef>
#include <cstdio>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

#include "tbl_copy.h"
#include "tbl_item.h"
#include "tbl_matchmaking.h"
#include "tbl_npc.h"
#include "tbl_skill_level.h"

using namespace server::table;

static int convertEncoding(
    const char *from, const char *to,
    char *in_buffer, size_t in_buffer_size,
    char *out_buffer, size_t out_buffer_size)
{
    iconv_t cd;

    cd = iconv_open(to, from);
    if ((iconv_t)(-1) == cd) {
        return -1;
    }

    char *in = in_buffer;
    char *out = out_buffer;
    size_t in_size = in_buffer_size;
    size_t out_size = out_buffer_size;

    size_t ret = iconv(cd, &in, &in_size, &out, &out_size);
    if ((size_t)-1 == ret) {
        iconv_close(cd);
        return -1;
    }

    iconv_close(cd);

    return out_buffer_size - out_size;
}

static std::string getTableFileContent(const std::string &file_path)
{
    std::ifstream fs(file_path.c_str(), std::ios::binary | std::ios::ate);
    if (fs.is_open() == false) {
        ::fprintf(stderr, "can not open file %s\n",
            file_path.c_str());
        return "";
    }

    std::vector<char> input_buffer(fs.tellg());
    fs.seekg(0);
    input_buffer.assign((std::istreambuf_iterator<char>(fs)),
                         std::istreambuf_iterator<char>());

    std::vector<char> output_buffer(input_buffer.size() * 2);

    int size = convertEncoding("UTF-16", "UTF-8",
        &input_buffer[0], input_buffer.size(),
        &output_buffer[0], output_buffer.size());
    if (-1 == size) {
        ::fprintf(stderr,
            "convert file %s encoding from utf-16 to utf-8 failed\n",
            file_path.c_str());
        return "";
    }

    return std::string(&output_buffer[0]);
}

int main(int argc, char *argv[])
{
    std::string csv_dir = ".";
    if (argc > 1) {
        csv_dir = argv[1];
    }

    TblCopy tbl_copy;
    TblItem tbl_item;
    TblMatchmaking tbl_matchmaking;
    TblNpc tbl_npc;
    TblSkillLevel tbl_skill_level;
    std::string error_info;

    if (tbl_copy.parse(
            getTableFileContent(csv_dir + "/copy.csv"),
            &error_info) == false) {
        ::fprintf(stderr, "parse %s failed: %s\n",
            "copy.csv", error_info.c_str());
        return 1;
    }
    if (tbl_item.parse(
            getTableFileContent(csv_dir + "/item.csv"),
            &error_info) == false) {
        ::fprintf(stderr, "parse %s failed: %s\n",
            "item.csv", error_info.c_str());
        return 1;
    }
    if (tbl_matchmaking.parse(
            getTableFileContent(csv_dir + "/matchmaking.csv"),
            &error_info) == false) {
        ::fprintf(stderr, "parse %s failed: %s\n",
            "matchmaking.csv", error_info.c_str());
        return 1;
    }
    if (tbl_npc.parse(
            getTableFileContent(csv_dir + "/npc.csv"),
            &error_info) == false) {
        ::fprintf(stderr, "parse %s failed: %s\n",
            "npc.csv", error_info.c_str());
        return 1;
    }
    if (tbl_skill_level.parse(
            getTableFileContent(csv_dir + "/skill_level.csv"),
            &error_info) == false) {
        ::fprintf(stderr, "parse %s failed: %s\n",
            "skill_level.csv", error_info.c_str());
        return 1;
    }

    {
        const TblMatchmaking::Row *row =
            tbl_matchmaking.getRow(3);
        if (row != NULL) {
            ::printf("tbl_matchmaking:3:max_count: %d\n",
                row->max_count);
        }
    }

    {
        const TblSkillLevel::RowSet *row_set =
            tbl_skill_level.getRowSet(100503);
        if (row_set != NULL) {
            ::printf("tbl_skill_level:100503:range_param:p1: %d\n",
                (*row_set)[0].range_param.p1);
        }
    }

    return 0;
}
