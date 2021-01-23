#pragma once
#include "DeviceResources.h"
#include "Interoperation/InteroperationTypes.h"
#include "GraphicsSignature.h"
#include "ShaderMacro.h"
namespace Coocoo3DGraphics
{
	using namespace Windows::Storage::Streams;
	public ref class ComputePO sealed
	{
	public:
		property GraphicsObjectStatus Status;
		//ʹ��Upload�ϴ�GPU
		bool CompileInitialize1(IBuffer^ file1, Platform::String^ entryPoint, ShaderMacro macro);
		void Initialize(DeviceResources^ deviceResources,GraphicsSignature^ rootSignature, IBuffer^ data);
		bool Upload(DeviceResources^ deviceResources, GraphicsSignature^ rootSignature);
		void Initialize(IBuffer^ data);
	internal:
		Microsoft::WRL::ComPtr<ID3D12PipelineState> m_pipelineState;
		Microsoft::WRL::ComPtr<ID3DBlob> byteCode;
	};
}
