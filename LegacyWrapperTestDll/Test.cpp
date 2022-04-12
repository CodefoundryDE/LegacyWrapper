#include <stdio.h>
#include <wchar.h>
#include <wtypes.h>
#include <Windows.h>

#define DllExport __declspec(dllexport)

extern "C" {

	DllExport __int32 __stdcall TestStdCall(const __int32 AParam)
	{
		return AParam;
	}

	DllExport __int32 _cdecl TestNormalFunc(const __int32 AParam)
	{
		return AParam;
	}

	DllExport char* TestPCharHandling(char* AParam)
	{
		size_t size = sizeof(char) * strlen(AParam) + 1;
		char* ptr = (char*) CoTaskMemAlloc(size);

		if (ptr != NULL) 
		{
			strncpy(ptr, AParam, size);
		}

		return ptr;
	}

	DllExport wchar_t* TestPWideCharHandling(wchar_t* AParam)
	{
		size_t size = sizeof(wchar_t) * wcslen(AParam) + 1;
		wchar_t* ptr = (wchar_t*)CoTaskMemAlloc(size);

		if (ptr != NULL) {
			wcsncpy(ptr, AParam, size);
		}

		return ptr;
	}
	
	DllExport int TestBstrHandling(BSTR AParam) {
		size_t size = sizeof(wchar_t) * SysStringLen(AParam) + 1;

		return size;
	}

	DllExport int TestBstrPointerHandling(BSTR* AParam) {
		size_t size = sizeof(wchar_t) * SysStringLen(*AParam) + 1;

		return size;
	}

	DllExport int TestBstrRefHandling(BSTR* AParam) {
		size_t size = sizeof(wchar_t) * SysStringLen(*AParam) + 1;

		*AParam = SysAllocString(L"HELLO WORLD!");

		return size;
	}

	/////

	DllExport void TestVarParamHandling(__int32 &AParam)
	{
		AParam = AParam + 1;
	}

	DllExport void TestMultipleVarParamsHandling(__int32 &AParam1, __int32 &AParam2)
	{
		AParam1 = AParam1 + 1;
		AParam2 = AParam2 + 1;
	}

}