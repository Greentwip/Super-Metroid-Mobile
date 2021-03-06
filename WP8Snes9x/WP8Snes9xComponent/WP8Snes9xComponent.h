#pragma once

#include "pch.h"
#include "BasicTimer.h"
#include "EmulatorRenderer.h"
#include "VirtualController.h"
#include "EmulatorSettings.h"
#include "EmulatorFileHandler.h"
#include <DrawingSurfaceNative.h>

using namespace Emulator;

namespace PhoneDirect3DXamlAppComponent
{
public delegate void RequestAdditionalFrameHandler();
public delegate void ContinueEmulationNotifier(void);
public delegate void SnapshotCallback(const Platform::Array<unsigned short> ^pixelData, int pitch, Platform::String ^fileName);
public delegate void SavestateCreatedCallback(int slot, Platform::String ^romFileName);
public delegate void SavestateSelectedCallback(int newSlot, int oldSlot);

[Windows::Foundation::Metadata::WebHostHidden]
public ref class Direct3DBackground sealed : public Windows::Phone::Input::Interop::IDrawingSurfaceManipulationHandler
{	
public:
	Direct3DBackground();

	Windows::Phone::Graphics::Interop::IDrawingSurfaceBackgroundContentProvider^ CreateContentProvider();

	// IDrawingSurfaceManipulationHandler
	virtual void SetManipulationHost(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ manipulationHost);

	event RequestAdditionalFrameHandler^ RequestAdditionalFrame;

	property StorageFile ^LoadadROMFile
	{
		StorageFile ^get()
		{
			return ROMFile;
		}
	}

	property int SelectedSavestateSlot
	{
		int get()
		{
			return SavestateSlot;
		}
	}

	property SavestateSelectedCallback ^SavestateSelected;
	property SnapshotCallback ^SnapshotAvailable;
	property SavestateCreatedCallback ^SavestateCreated;
	property Windows::Foundation::Size WindowBounds;
	property Windows::Foundation::Size NativeResolution;
	property Windows::Foundation::Size RenderResolution;

	void ToggleTurboMode(void);
	void StartTurboMode(void);
	void StopTurboMode(void);

	void TriggerSnapshot(void);

	void SelectSaveState(int slot);
	void SaveState(void);
	void LoadState(void);
	void Reset(void);
	void SetContinueNotifier(ContinueEmulationNotifier ^notifier);

	void ChangeOrientation(int orientation);
	bool IsROMLoaded(void);
	void PauseEmulation(void);
	void UnpauseEmulation(void);
	void LoadROMAsync(Windows::Storage::StorageFile ^file, Windows::Storage::StorageFolder ^folder);

protected:
	// Event Handlers
	void OnPointerPressed(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);
	void OnPointerReleased(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);
	void OnPointerMoved(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);

internal:
	HRESULT Connect(_In_ IDrawingSurfaceRuntimeHostNative* host, _In_ ID3D11Device1* device);
	void Disconnect();

	HRESULT PrepareResources(_In_ const LARGE_INTEGER* presentTargetTime, _Inout_ DrawingSurfaceSizeF* desiredRenderTargetSize);
	HRESULT Draw(_In_ ID3D11Device1* device, _In_ ID3D11DeviceContext1* context, _In_ ID3D11RenderTargetView* renderTargetView);

private:
	ContinueEmulationNotifier ^ContinueEmulationNotifier;
	EmulatorRenderer^ m_renderer;
	BasicTimer^ m_timer;
	EmulatorGame *emulator;
	VirtualController *vController;
	int orientation;
};

}