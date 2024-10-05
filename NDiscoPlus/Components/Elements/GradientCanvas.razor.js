// Heavy inspiration: https://adammurray.blog/webgl/tutorials/

const vertices = [
    [-1, -1],
    [1, -1],
    [-1, 1],
    [1, 1],
];
const vertexData = new Float32Array(vertices.flat());

/**
 * @param {HTMLElement} divElementReference
 * @param {number} width 
 * @param {number} height
 * @param {bool} useHDR
 * @param {Map<String, String>} fragmentShaderArgs 
 */
export function createShaderPipeline(divElementReference, width, height, useHDR, fragmentShaderArgs) {
    if (divElementReference === null) {
        // Sometimes this is called when the website has already started unloading.
        // In this case, C#-land still has the element reference, but JS-topia doesn't already.
        // Let's exit this rare case graciously.
        return null;
    }

    const canvas = divElementReference.querySelector("#GradientCanvas_canvas");
    const vertexShaderSource = divElementReference.querySelector("#GradientCanvas_vertex").textContent;

    let fragmentShaderSourceBuilder = divElementReference.querySelector("#GradientCanvas_fragment").textContent;
    for (const fragmentArg of Object.entries(fragmentShaderArgs)) {
        fragmentShaderSourceBuilder = fragmentShaderSourceBuilder.replace(fragmentArg[0], fragmentArg[1]);
    }
    const fragmentShaderSource = fragmentShaderSourceBuilder;

    return createProgram(canvas, width, height, useHDR, vertexShaderSource, fragmentShaderSource);
}

/**
 * @param {HTMLCanvasElement} canvas
 * @param {number} width 
 * @param {number} height 
 * @param {String} vertexCode
 * @param {String} fragmentCode
 */
function createProgram(canvas, width, height, useHDR, vertexCode, fragmentCode) {
    const gl = canvas.getContext("webgl2");
    if (!gl) throw "WebGL2 not supported";

    gl.viewport(0, 0, width, height);
    if (useHDR) {
        throw "Not implemented.";
    }

    function createShader(shaderType, sourceCode) {
        const shader = gl.createShader(shaderType);
        gl.shaderSource(shader, sourceCode.trim());
        gl.compileShader(shader);
        if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
            throw gl.getShaderInfoLog(shader);
        }
        return shader;
    }

    const program = gl.createProgram();
    gl.attachShader(program, createShader(gl.VERTEX_SHADER, vertexCode));
    gl.attachShader(program, createShader(gl.FRAGMENT_SHADER, fragmentCode));
    gl.linkProgram(program);
    if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
        throw gl.getProgramInfoLog(program);
    }
    gl.useProgram(program);

    gl.bindBuffer(gl.ARRAY_BUFFER, gl.createBuffer());
    gl.bufferData(gl.ARRAY_BUFFER, vertexData, gl.STATIC_DRAW);

    const vertexPosition = gl.getAttribLocation(program, "vertexPosition");
    gl.enableVertexAttribArray(vertexPosition);
    gl.vertexAttribPointer(vertexPosition, 2, gl.FLOAT, false, 0, 0);

    const canvasSizeUniform = gl.getUniformLocation(program, "canvasSize");
    gl.uniform2f(canvasSizeUniform, canvas.width, canvas.height);

    return new WebGL2Program(gl, program);
}

/**
 * @param {WebGL2Program} p
 */
function renderProgram(p) {
    if (p.colors === null) throw "No colors set.";

    p.gl.uniform3fv(p.lightColorsUnion, p.colors);
    p.gl.drawArrays(p.gl.TRIANGLE_STRIP, 0, vertices.length);

    if (p.rendering) {
        requestAnimationFrame(() => renderProgram(p));
    }
}

class WebGL2Program {
    /**@type {bool}*/
    rendering;

    /**@type {?number[]}*/
    colors;

    /**
     * @param {WebGL2RenderingContext} gl
     * @param {WebGL2Program} program
     */
    constructor(gl, program) {
        this.gl = gl;
        this.program = program;

        this.lightColorsUnion = gl.getUniformLocation(program, "lightColors");

        this.rendering = false;
        this.colors = null;
    }

    /**
     * 
     * @param {number[]} colors
     */
    set_colors(colors) {
        this.colors = colors;
    }

    /**
     * 
     * @param {number[]} colors
     */
    start_render(colors) {
        this.colors = colors;

        if (!this.rendering) {
            this.rendering = true;
            renderProgram(this);
        }
    }

    stop_render() {
        if (this.rendering) {
            this.rendering = false;
        }
    }

    dispose() {
        this.stop_render();
        this.gl.deleteProgram(this.program);
    }
}