#pragma once
#include "ITexture.h"
#include "Interoperation/InteroperationTypes.h"
namespace Coocoo3DGraphics
{
	public ref class RenderTextureCube sealed :public IRenderTexture, public ITextureCube
	{
	public:
		void ReloadAsRTVUAV(int width, int height,int mipLevels, DxgiFormat format);
		void ReloadAsDSV(int width, int height, DxgiFormat format);
		property UINT m_width;
		property UINT m_height;
		property UINT m_mipLevels;
	internal:
		Microsoft::WRL::ComPtr<ID3D12Resource> m_texture;
		UINT m_dsvHeapRefIndex;
		UINT m_rtvHeapRefIndex;
		DXGI_FORMAT m_format;
		DXGI_FORMAT m_dsvFormat;
		DXGI_FORMAT m_rtvFormat;
		DXGI_FORMAT m_uavFormat;
		D3D12_RESOURCE_FLAGS m_resourceFlags;
		D3D12_RESOURCE_STATES prevResourceState;
	};
}
