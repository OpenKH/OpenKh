#include <map>
#include <string>
#include <fstream>
#include <iostream>
#include <filesystem>
#include <Windows.h>

using namespace std;

static map<string, string> MAP_NAMES =
{
    {"kh1", "KINGDOM HEARTS FINAL MIX.exe" },
    {"kh2", "KINGDOM HEARTS II FINAL MIX.exe" },
    {"recom", "KINGDOM HEARTS Re_Chain of Memories.exe" },
    {"bbs", "KINGDOM HEARTS Birth by Sleep FINAL MIX.exe" },
    {"kh3d", "KINGDOM HEARTS Dream Drop Distance.exe" },
};

static string SELF_NAME = "KINGDOM HEARTS HD 1.5+2.5 ReMIX.exe";
static string REAL_NAME = "KINGDOM HEARTS HD 1.5+2.5 ReMIX.bak";

static string ARGUMENT_FETCH = "launch_args.txt";

int main(int argc, char* argv[])
{
    auto _fetchHandle = GetConsoleWindow();
    ShowWindow(_fetchHandle, SW_HIDE);

    wchar_t _fetchPath[MAX_PATH];

    auto _fetchLength = GetModuleFileNameW(nullptr, _fetchPath, MAX_PATH);
    auto _fetchPathFS = filesystem::path(_fetchPath).parent_path().string();

    auto _fetchArgsPath = _fetchPathFS + "\\" + ARGUMENT_FETCH;

    auto _fetchSelfPath = "\"" + _fetchPathFS + "\\" + SELF_NAME + "\"";
    auto _fetchRealPath = "\"" + _fetchPathFS + "\\" + REAL_NAME + "\"";

    string _makeCommand;

    if (filesystem::exists(_fetchArgsPath))
    {
        ifstream _fetchFile(_fetchArgsPath);

        stringstream _fetchBuff;
        _fetchBuff << _fetchFile.rdbuf();

        string _currArg;
        vector<string> _fetchArgs;

        while (getline(_fetchBuff, _currArg, ' '))
            _fetchArgs.push_back(_currArg);

        if (_fetchArgs.size() >= 0x01)
        {
            if (_fetchArgs[0] == "kh3d")
            {
                SELF_NAME = "KINGDOM HEARTS HD 2.8 Final Chapter Prologue.exe";
                REAL_NAME = "KINGDOM HEARTS HD 2.8 Final Chapter Prologue.bak";

                _fetchSelfPath = "\"" + _fetchPathFS + "\\" + SELF_NAME + "\"";
                _fetchRealPath = "\"" + _fetchPathFS + "\\" + REAL_NAME + "\"";
            }

            _makeCommand = "\"" + _fetchPathFS + "\\" + MAP_NAMES[_fetchArgs[0]] + "\"";

            for (int i = 1; i < _fetchArgs.size(); i++)
            {
                _makeCommand += " ";
                _makeCommand += _fetchArgs[i];
            }

            for (int i = 1; i < argc; i++)
            {
                if (argv[i] == "%command%")
                    continue;

                _makeCommand += " ";
                _makeCommand += argv[i];
            }

            system(_makeCommand.c_str());
        }
    }

    if (filesystem::exists(_fetchArgsPath))
        filesystem::remove(_fetchArgsPath);

    system(string("RENAME " + _fetchSelfPath + " \"delete.this\"").c_str());
    system(string("RENAME " + _fetchRealPath + " \"" + SELF_NAME + "\"").c_str());

    return 0;
}
