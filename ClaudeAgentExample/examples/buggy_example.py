
def calculate_average(numbers):
    total = 0
    for num in numbers:
        total += num
    average = total / len(numbers)
    return average

# 测试
result = calculate_average([1, 2, 3, 4, 5])
print(f"平均值: {result}")

# 边界情况测试
empty_result = calculate_average([])
print(f"空列表平均值: {empty_result}")
