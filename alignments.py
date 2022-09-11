import itertools
import re

def size_for_version(version):
    return 17 + 4 * version

def alignment_coord_list(version):
    divs = 2 + version // 7
    size = size_for_version(version)
    total_dist = size - 7 - 6
    divisor = 2 * (divs - 1)
    # Step must be even, for alignment patterns to agree with timing patterns
    step = (total_dist + divisor // 2 + 1) // divisor * 2 # Get the rounding right
    coords = [6]
    for i in range(divs - 2, -1, -1): # divs-2 down to 0, inclusive
        coords.append(size - 7 - i * step)

    permutations = list(itertools.product(coords, repeat=2))

    shift = size - 9

    allowedPermutations = []

    for (row, col) in permutations:
        if not (((row - 2 < 8) and (col - 2 < 8)) or ((row - 2 < 8) and (col + 2 > shift)) or ((row + 2 > shift) and (col - 2 < 8))):
            allowedPermutations.append((row, col))

    return allowedPermutations

for version in range(2, 40 + 1): # 1 to 40 inclusive
    print("new[]\n{")
    coords_list = alignment_coord_list(version)
    for coords in coords_list[:-1]:
        print(f"\t{coords},")
    print(f"\t{coords_list[len(coords_list) - 1]}")
    print("},")