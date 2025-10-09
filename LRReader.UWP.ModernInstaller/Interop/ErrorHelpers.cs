#nullable enable

using System;
using System.Runtime.CompilerServices;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace LRReader.UWP.Installer.Interop;

internal static unsafe class ErrorHelpers
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowIfWin32Error(uint value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is not 0)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(value));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfNull(void* value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is null)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfNull(nint value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is 0)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfNull(nuint value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is 0)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfNull(ulong value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is 0)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfNull(uint value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is 0)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfNull(ushort value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is 0)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfDefault<T>(T value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null) where T : IEquatable<T>
	{
		if (value.Equals(default))
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfFalse(bool value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value is false)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerHidden, System.Diagnostics.StackTraceHidden]
	internal static void ThrowLastErrorIfFalse(BOOL value, [CallerArgumentExpression(nameof(value))] string? valueExpression = null)
	{
		if (value.Value is 0)
		{
			ThrowExternalException(valueExpression ?? "Method", HRESULT_FROM_WIN32(GetLastError()));
		}
	}
}
