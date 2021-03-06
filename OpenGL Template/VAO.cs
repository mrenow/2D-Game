﻿using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Template {
    class VAO {
        public int ID { get; private set; }

        public int verticesID { get; private set; }
        public int coloursID { get; private set; }
        public int elementsID { get; private set; }

        public int count { get; private set; }

        public VAO(Vector3[] vertices, Color4[] colours, int[] elements) {

            ShaderProgram program = Renderer.shader;
            Debug.Assert(program != null);

            count = elements.Length;

            this.ID = GL.GenVertexArray();
            GL.BindVertexArray(ID);

            {
                this.verticesID = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, verticesID);
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(3 * sizeof(float) * vertices.Length), vertices, BufferUsageHint.StaticDraw);
                uint vertexAttribLocation = (uint)GL.GetAttribLocation(program.id, "v_pos");
                GL.VertexAttribPointer(vertexAttribLocation, 3, VertexAttribPointerType.Float, true, 3 * sizeof(float), IntPtr.Zero);
                GL.EnableVertexAttribArray(vertexAttribLocation);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                this.coloursID = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(4 * sizeof(float) * colours.Length), colours, BufferUsageHint.StaticDraw);
                uint colourAttribLocation = (uint)GL.GetAttribLocation(program.id, "v_colour");
                GL.VertexAttribPointer(colourAttribLocation, 4, VertexAttribPointerType.Float, true, 4 * sizeof(float), IntPtr.Zero);
                GL.EnableVertexAttribArray(colourAttribLocation);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                this.elementsID = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementsID);
                GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(int) * elements.Length), elements, BufferUsageHint.StaticDraw);
            }

            GL.BindVertexArray(0);
        }

        public void UpdateVertexData(Vector3[] vertices) {
            GL.BindVertexArray(ID);

            ShaderProgram shader = Renderer.shader;
            Debug.Assert(shader != null);

            GL.DeleteBuffer(verticesID);
            this.verticesID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, verticesID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(3 * sizeof(float) * vertices.Length), vertices, BufferUsageHint.StreamDraw);
            uint vertexAttribLocation = (uint)GL.GetAttribLocation(shader.id, "v_pos");
            GL.VertexAttribPointer(vertexAttribLocation, 3, VertexAttribPointerType.Float, true, 3 * sizeof(float), IntPtr.Zero);
            GL.EnableVertexAttribArray(vertexAttribLocation);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }
        public void UpdateElementData(int[] elements) {
            GL.BindVertexArray(ID);

            ShaderProgram shader = Renderer.shader;
            Debug.Assert(shader != null);

            GL.DeleteBuffer(elementsID);
            this.elementsID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementsID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(int) * elements.Length), elements, BufferUsageHint.StreamDraw);
            count = elements.Length;

            GL.BindVertexArray(0);
        }
        public void UpdateColourData(Vector4[] colours) {
            GL.BindVertexArray(ID);

            ShaderProgram shader = Renderer.shader;
            Debug.Assert(shader != null);

            GL.DeleteBuffer(coloursID);
            this.verticesID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(4 * sizeof(float) * colours.Length), colours, BufferUsageHint.StreamDraw);
            uint colourAttribLocation = (uint)GL.GetAttribLocation(shader.id, "v_colour");
            GL.VertexAttribPointer(colourAttribLocation, 4, VertexAttribPointerType.Float, true, 4 * sizeof(float), IntPtr.Zero);
            GL.EnableVertexAttribArray(colourAttribLocation);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }



        public void DisposeAll() {
            GL.DeleteBuffer(verticesID);
            GL.DeleteBuffer(coloursID);
            GL.DeleteBuffer(elementsID);
            GL.DeleteVertexArrays(1, new int[] { ID });
        }
    }
}
