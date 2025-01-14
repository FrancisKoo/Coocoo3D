#include "pch.h"
#include "RenderTexture2D.h"
#include "DirectXHelper.h"
using namespace Coocoo3DGraphics;

void RenderTexture2D::ReloadAsDepthStencil(int width, int height, DxgiFormat format)
{
	m_width = width;
	m_height = height;
	if ((DXGI_FORMAT)format == DXGI_FORMAT_D24_UNORM_S8_UINT)
		m_format = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
	else if ((DXGI_FORMAT)format == DXGI_FORMAT_D32_FLOAT)
		m_format = DXGI_FORMAT_R32_FLOAT;
	m_dsvFormat = (DXGI_FORMAT)format;
	m_rtvFormat = DXGI_FORMAT_UNKNOWN;
	m_resourceFlags = D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
}

void RenderTexture2D::ReloadAsRenderTarget(int width, int height, DxgiFormat format)
{
	m_width = width;
	m_height = height;
	m_format = (DXGI_FORMAT)format;
	m_dsvFormat = DXGI_FORMAT_UNKNOWN;
	m_rtvFormat = (DXGI_FORMAT)format;
	m_uavFormat = DXGI_FORMAT_UNKNOWN;
	m_resourceFlags = D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
}

void RenderTexture2D::ReloadAsRTVUAV(int width, int height, DxgiFormat format)
{
	m_width = width;
	m_height = height;
	m_format = (DXGI_FORMAT)format;
	m_dsvFormat = DXGI_FORMAT_UNKNOWN;
	m_rtvFormat = (DXGI_FORMAT)format;
	m_uavFormat = (DXGI_FORMAT)format;
	m_resourceFlags = D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET | D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
}

DxgiFormat Coocoo3DGraphics::RenderTexture2D::GetFormat()
{
	if (m_dsvFormat != DXGI_FORMAT_UNKNOWN)
		return (DxgiFormat)m_dsvFormat;
	return (DxgiFormat)m_format;
}

void RenderTexture2D::StateTransition(ID3D12GraphicsCommandList* commandList, D3D12_RESOURCE_STATES state)
{
	if (prevResourceState != state)
		commandList->ResourceBarrier(1, &CD3DX12_RESOURCE_BARRIER::Transition(m_texture.Get(), prevResourceState, state));
	prevResourceState = state;
}
