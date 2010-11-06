// DlrComStopwatch.cpp : Implementation of CDlrComStopwatch

#include "stdafx.h"
#include "DlrComStopwatch.h"


// CDlrComStopwatch


STDMETHODIMP CDlrComStopwatch::Start(void)
{
	if (QueryPerformanceCounter(&_startCount) == 0)
	{
		return E_FAIL;
	}

	return S_OK;
}

STDMETHODIMP CDlrComStopwatch::get_ElapsedSeconds(DOUBLE* pVal)
{
	LARGE_INTEGER endCount;
	if (QueryPerformanceCounter(&endCount) == 0)
	{
		return E_FAIL;
	}

	LONGLONG elapsedCount = endCount.QuadPart - _startCount.QuadPart;

	LARGE_INTEGER frequency;
	if (QueryPerformanceFrequency(&frequency) == 0)
	{
		return E_FAIL;
	}

	*pVal = elapsedCount / ((DOUBLE)frequency.QuadPart);

	return S_OK;
}
