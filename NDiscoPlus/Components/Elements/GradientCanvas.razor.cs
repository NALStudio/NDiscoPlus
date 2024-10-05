using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;
using System.Globalization;

namespace NDiscoPlus.Components.Elements;

public partial class GradientCanvas : IAsyncDisposable
{
    private readonly record struct ShaderArgs(int ColorCount, bool UseHDR, bool Dither)
    {
        public Dictionary<string, string> ToArgumentDictionary()
        {
            static string BoolToGlslConstant(bool value) => value ? "true" : "false";
            static string IntToGlslConstant(int value) => value.ToString(CultureInfo.InvariantCulture);

            (int colorCount, bool useHdr, bool dither) = this;

            if (colorCount != 4 && colorCount != 6)
                throw new InvalidOperationException("Invalid color count.");

            return new()
            {
                { "%LIGHT_COUNT%", IntToGlslConstant(colorCount) },
                { "%USE_HDR%", BoolToGlslConstant(useHdr) },
                { "%DITHERED%", BoolToGlslConstant(dither) },
            };
        }
    }

    private readonly record struct SizeArgs(int Width, int Height);

    /// <summary>
    /// Either 4 or 6 colors to render.
    /// </summary>
    /// <remarks>
    /// Do not change the color count often as it forces a rebuild of the entire shader pipeline.
    /// </remarks>
    [Parameter, EditorRequired]
    public IReadOnlyList<NDPColor>? Colors { get; set; }

    [Parameter, EditorRequired]
    public int Width { get; set; }

    [Parameter, EditorRequired]
    public int Height { get; set; }

    [Parameter]
    public bool UseHDR { get; set; } = false;
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
        Debug.Assert(Colors is not null);

        ShaderArgs shaderArgs = new(Colors.Count, UseHDR, Dither);
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
                colors: Colors
            );
            program.ContinueWith(_ => StateHasChanged(), TaskContinuationOptions.ExecuteSynchronously);
            // execute synchronously because Blazor doesn't support multithreading

            previousShaderArgs = shaderArgs;
            previousSizeArgs = sizeArgs;
        }
    }

    private async Task<IJSObjectReference?> CreateProgram(Task<IJSObjectReference?>? previousProgram, ShaderArgs shaderArgs, SizeArgs sizeArgs, IReadOnlyList<NDPColor> colors)
    {
        if (previousProgram is not null)
            await DisposeProgram(previousProgram);

        IJSObjectReference module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Elements/GradientCanvas.razor.js");

        IJSObjectReference? program = await module.InvokeAsync<IJSObjectReference?>("createShaderPipeline", ParentDivReference, sizeArgs.Width, sizeArgs.Height, shaderArgs.UseHDR, shaderArgs.ToArgumentDictionary());
        if (program is null) // see javascript file for null return justification
            return null;

        await program.InvokeVoidAsync("start_render", UnpackColors(colors));

        return program;
    }

    protected override async Task OnParametersSetAsync()
    {
        Debug.Assert(Colors is not null);

        if (program?.IsCompleted == true && program.Result is IJSObjectReference prog)
            await prog.InvokeVoidAsync("set_colors", UnpackColors(Colors));
    }

    private static IEnumerable<double> UnpackColors(IEnumerable<NDPColor> colors)
    {
        foreach (NDPColor color in colors)
        {
            yield return color.X;
            yield return color.Y;
            yield return color.Brightness;
        }
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
