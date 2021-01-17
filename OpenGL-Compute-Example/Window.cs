using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenGL_Compute_Example
{
    public class Window : GameWindow
    {
        private const int NumParticles = 7000000;
        private DateTime lastFrameTime = DateTime.Now;
        private int renderProgram;
        private int computeProgram;
        private int cursorUniformLocation;
        private int deltaTimeUniformLocation;


        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            createComputeBuffers();
            createShaders();

            GL.ClearColor(.05f, 0.05f, 0.05f, 1.0f);
            GL.PointSize(2.0f);

            // Basic native additive blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
        }

        private void createShaders()
        {
            // Render Program
            var vertexShaderSource = File.ReadAllText("shader.vert");
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            CompileShader(vertexShader);

            var fragmentShaderSource = File.ReadAllText("shader.frag");
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            CompileShader(fragmentShader);

            renderProgram = GL.CreateProgram();
            GL.AttachShader(renderProgram, vertexShader);
            GL.AttachShader(renderProgram, fragmentShader);
            LinkProgram(renderProgram);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Compute Program
            var computeShaderSource = File.ReadAllText("cursor.glsl");

            var computeShader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(computeShader, computeShaderSource);
            CompileShader(computeShader);

            computeProgram = GL.CreateProgram();
            GL.AttachShader(computeProgram, computeShader);
            LinkProgram(computeProgram);
            GL.DeleteShader(computeShader);

            GL.UseProgram(computeShader);
            cursorUniformLocation = GL.GetUniformLocation(computeProgram, "cursor");
            deltaTimeUniformLocation = GL.GetUniformLocation(computeProgram, "dt");
        }

        protected static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var error = GL.GetShaderInfoLog(shader);
                Console.WriteLine(error);
                throw new Exception($"Error occurred whilst compiling Shader({shader})");
            }
        }

        protected static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                var error = GL.GetProgramInfoLog(program);
                Console.WriteLine(error);
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        private void createComputeBuffers()
        {
            var positions = new Vector4[NumParticles];
            var velocities = new Vector4[NumParticles];
            var r = new Random();
            for (var i=0; i<NumParticles; i++)
            {
                // Random positions in range [-1, 1]
                var posx = (float)r.NextDouble() * 2 - 1;
                var posy = (float)r.NextDouble() * 2 - 1;
                positions[i] = new Vector4(posx, posy, 0, 0);
                velocities[i] = new Vector4(0,0,0,0);
            }

            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            var buffers = new int[2];
            GL.GenBuffers(buffers.Length, buffers);
            var pos = buffers[0];
            var vel = buffers[1];

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, pos);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, SizeInBytes(positions), positions, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, vel);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, SizeInBytes(velocities), velocities, BufferUsageHint.StaticDraw);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            var now = DateTime.Now;
            var frameDetltaTime = now - lastFrameTime;
            var timeScaler = (float)frameDetltaTime.TotalMilliseconds / (1000 / 60f);
            lastFrameTime = now;

            var cursor = new Vector2(MouseState.Position.X / Size.X, MouseState.Position.Y / Size.Y);
            cursor.Y = 1 - cursor.Y;
            cursor = (cursor - new Vector2(0.5f)) * 2f; // Transform to NDC space [-1, 1]
            if (cursor.X < 1 && cursor.Y < 1 && cursor.X > -1 && cursor.Y > -1)
            {
                GL.UseProgram(computeProgram);
                GL.Uniform2(cursorUniformLocation, cursor);
                timeScaler *= 0.1f;
                GL.Uniform1(deltaTimeUniformLocation, timeScaler);
                GL.DispatchCompute(NumParticles / 1000, 1, 1);
            }

            GL.UseProgram(renderProgram);
            GL.DrawArraysInstanced(PrimitiveType.Points, 0, 1, NumParticles);

            SwapBuffers();
        }

        private static int SizeInBytes<T>(T[] self) => Marshal.SizeOf<T>() * self.Length;
    }
}
