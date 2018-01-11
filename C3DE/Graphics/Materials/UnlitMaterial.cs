﻿using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class UnlitMaterial : Material
    {
        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                m_ShaderMaterial = new ForwardUnlit(this);

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
