// Antumbra.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#pragma region INCLUDES

#include <cstdio>
#include <cstdlib>
#include <string>
#include <iostream>

#include "Antumbra.h"

#pragma endregion

#pragma region USING DIRECTIVES

using namespace System;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Diagnostics;
using namespace System::Threading::Tasks;

#pragma endregion

const char* Antumbra::ApplicationName = "Antumbra CLI";
const char* Antumbra::ApplicationVersion = "V1.0-DEVELOPMENT";

static Antumbra A;
static std::stringstream ss;

auto Antumbra::PrintBuildInformation() {
	auto a = Assembly::GetExecutingAssembly();
	auto fvi = gcnew FileInfo(a->Location);

	ss << ApplicationName << ApplicationVersion << std::endl;

	Console::WriteLine(a + "\n\n");
	Console::WriteLine(ApplicationName);
	Console::WriteLine(ApplicationVersion);
}

int main(char* argv, int argc) {
	A.PrintBuildInformation();

	Console::ReadKey();
	return 0;
}