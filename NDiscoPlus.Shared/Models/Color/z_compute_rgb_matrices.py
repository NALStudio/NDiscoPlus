from typing import Any, Self, TypeVar, cast

# pip install numpy
import numpy

_T = TypeVar("_T")

dtype: type[numpy.floating[Any]] = numpy.float64

def xyY2XYZ(xy: tuple[float, float], Y: float = 1.0) -> tuple[float, float, float]:
    x = xy[0]
    y = xy[1]
    z = 1.0 - x - y

    X = (Y / y) * x
    Z = (Y / y) * z

    return (X, Y, Z)

def create_matrix(array: list[list[float]]) -> numpy.matrix[float, Any]:
    return numpy.matrix(array, dtype=dtype)

def no_op(x: _T) -> _T:
    return x

def matrix(r_xy: tuple[float, float], g_xy: tuple[float, float], b_xy: tuple[float, float], white_point_xy: tuple[float, float]):
    r_XYZ = xyY2XYZ(r_xy)
    g_XYZ = xyY2XYZ(g_xy)
    b_XYZ = xyY2XYZ(b_xy)

    white_point_XYZ = xyY2XYZ(white_point_xy)

    x = create_matrix([
        [r_XYZ[0], g_XYZ[0], b_XYZ[0]],
        [r_XYZ[1], g_XYZ[1], b_XYZ[1]],
        [r_XYZ[2], g_XYZ[2], b_XYZ[2]]
    ])
    w = create_matrix([
        [white_point_XYZ[0]],
        [white_point_XYZ[1]],
        [white_point_XYZ[2]]
    ])

    s = x.I * w

    sR: float = s[0, 0]
    sG: float = s[1, 0]
    sB: float = s[2, 0]

    # wrap with another numpy.matrix as .round() returns numpy.ndarray for some reason
    return numpy.matrix(create_matrix([
        [sR * r_XYZ[0], sG * g_XYZ[0], sB * b_XYZ[0]],
        [sR * r_XYZ[1], sG * g_XYZ[1], sB * b_XYZ[1]],
        [sR * r_XYZ[2], sG * g_XYZ[2], sB * b_XYZ[2]]
    ]).round(4), dtype=dtype)

# Values from: https://en.wikipedia.org/wiki/SRGB#Gamut
def rgb():
    r_xy = (0.6400, 0.3300)
    g_xy = (0.3000, 0.6000)
    b_xy = (0.1500, 0.0600)

    white_point_xy = (0.3127, 0.3290)

    return matrix(r_xy, g_xy, b_xy, white_point_xy)

# values from: https://en.wikipedia.org/wiki/DCI-P3#P3_colorimetry
def display_p3():
    r_xy = (0.680, 0.320)
    g_xy = (0.265, 0.690)
    b_xy = (0.150, 0.060)

    white_point_xy = (0.3127, 0.3290)

    return matrix(r_xy, g_xy, b_xy, white_point_xy)

rgb_matrix = rgb()
display_p3_matrix = display_p3()

# RGB MATRIX COMPUTED FOR CROSS REFERENCE WITH: https://www.color.org/chardata/rgb/sRGB.pdf
output = f"""
RGB Forward:
{rgb_matrix}

RGB Inverse:
{rgb_matrix.I.round(decimals=7)}

Display P3 Forward:
{display_p3_matrix}

Display P3 Inverse:
{display_p3_matrix.I.round(decimals=7)}
"""
print(output)
