using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;
using System.Globalization;

namespace NDiscoPlus.Components.Elements.GradientCanvas;

public partial class BaseGradientCanvas : IAsyncDisposable
{
    public readonly record struct GradientColors(int Count, UniformVector VectorType, double[] Unpacked);
    public enum UniformVector { vec3 = 3, vec4 = 4 }

    private readonly record struct ShaderArgs(int ColorCount, bool Dither)
    {
        public Dictionary<string, string> ToArgumentDictionary()
        {
            static string BoolToGlslConstant(bool value) => value ? "true" : "false";
            static string IntToGlslConstant(int value) => value.ToString(CultureInfo.InvariantCulture);

            (int colorCount, bool dither) = this;

            return new()
            {
                { "%LIGHT_COUNT%", IntToGlslConstant(colorCount) },
                { "%DITHERED%", BoolToGlslConstant(dither) }
            };
        }
    }

    private readonly record struct SizeArgs(int Width, int Height);

    [Parameter, EditorRequired]
    public RenderFragment? FragmentShader { get; set; }

    [Parameter, EditorRequired]
    public int Width { get; set; }

    [Parameter, EditorRequired]
    public int Height { get; set; }

    [Parameter, EditorRequired]
    public GradientColors? Colors { get; set; }

    [Parameter]
    public bool Dither { get; set; } = true;

    [Parameter]
    public string? Style { get; set; }

    protected ElementReference? ParentDivReference { get; set; }

    private ShaderArgs? previousShaderArgs;
    private SizeArgs? previousSizeArgs;

    private Task<IJSObjectReference?>? program;

    protected override void OnAfterRender(bool firstRender)
    {
        RecreateProgramIfNeeded();
    }

    private void RecreateProgramIfNeeded()
    {
        if (Colors is not GradientColors colors)
            throw new InvalidOperationException("No colors set.");

        ShaderArgs shaderArgs = new(colors.Count, Dither);
        SizeArgs sizeArgs = new(Width, Height);

        if (
            program is null
            || (program.IsCompleted && program.Result is null)
            || shaderArgs != previousShaderArgs
            || sizeArgs != previousSizeArgs
        )
        {
            program = CreateProgram(
                previousProgram: program,
                shaderArgs: shaderArgs,
                sizeArgs: sizeArgs,
                colors: colors
            );
            program.ContinueWith(_ => StateHasChanged(), TaskContinuationOptions.ExecuteSynchronously);
            // execute synchronously because Blazor doesn't support multithreading

            previousShaderArgs = shaderArgs;
            previousSizeArgs = sizeArgs;
        }
    }

    private async Task<IJSObjectReference?> CreateProgram(Task<IJSObjectReference?>? previousProgram, ShaderArgs shaderArgs, SizeArgs sizeArgs, GradientColors colors)
    {
        if (previousProgram is not null)
            await DisposeProgram(previousProgram);

        IJSObjectReference module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Elements/GradientCanvas/BaseGradientCanvas.razor.js");

        IJSObjectReference? program = await module.InvokeAsync<IJSObjectReference?>("createShaderPipeline", ParentDivReference, sizeArgs.Width, sizeArgs.Height, false, (int)colors.VectorType, shaderArgs.ToArgumentDictionary());
        if (program is null) // see javascript file for null return justification
            return null;

        await program.InvokeVoidAsync("start_render", colors.Unpacked);

        return program;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (program?.IsCompleted == true && program.Result is IJSObjectReference prog)
            await prog.InvokeVoidAsync("set_colors", Colors!.Value.Unpacked);
    }

    internal static async ValueTask DisposeProgram(Task<IJSObjectReference?> program)
    {
        IJSObjectReference? p = await program;
        if (p is not null)
        {
            await p.InvokeVoidAsync("dispose");
            await p.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (program is not null)
            await DisposeProgram(program);
    }
}
