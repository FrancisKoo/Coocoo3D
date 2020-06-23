// ����������ɫ�������ÿ����������ݡ�
struct VertexShaderInput
{
	float3 pos : POSITION;
	float2 tex : TEXCOORD;
	uint index : INDEX;
};

// ͨ��������ɫ�����ݵ�ÿ�����ص���ɫ���ݡ�
struct PixelShaderInput
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD;
	uint index : INDEX;
};

// ������ GPU ��ִ�ж��㴦��ļ���ɫ����
PixelShaderInput main(VertexShaderInput input)
{
	PixelShaderInput output;
	output.pos = float4(input.pos, 1);
	output.tex = input.tex;
	output.index = input.index;

	return output;
}
