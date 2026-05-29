"""@tool decorator — auto-generates schema from function signature."""

from __future__ import annotations

import inspect
from typing import get_type_hints

from heagent.tools.registry import ToolRegistry
from heagent.types import ToolSchema

_TYPE_MAP: dict[type, str] = {
    int: "integer",
    float: "number",
    bool: "boolean",
    str: "string",
}


def tool(
    func: object | None = None,
    *,
    name: str | None = None,
    description: str | None = None,
    enabled: bool = True,
) -> object:
    """Register a function as an Agent tool.

    Can be used as ``@tool`` or ``@tool(name="custom", enabled=False)``.
    Extracts name, docstring, and type annotations to build a ToolSchema.
    """

    def _register(fn: object) -> object:
        fn_obj = fn  # type: ignore[arg-type]
        tool_name = name or fn_obj.__name__
        tool_desc = description or (fn_obj.__doc__ or "").strip().split("\n")[0]

        hints = get_type_hints(fn_obj)
        sig = inspect.signature(fn_obj)

        properties: dict[str, object] = {}
        required: list[str] = []
        for param_name, param in sig.parameters.items():
            if param_name == "self":
                continue
            ptype = hints.get(param_name, str)
            prop: dict[str, object] = {"type": _TYPE_MAP.get(ptype, "string")}
            if param.default is not inspect.Parameter.empty:
                prop["default"] = param.default
            properties[param_name] = prop
            if param.default is inspect.Parameter.empty:
                required.append(param_name)

        parameters: dict[str, object] = {"type": "object", "properties": properties}
        if required:
            parameters["required"] = required

        schema = ToolSchema(name=tool_name, description=tool_desc, parameters=parameters)
        registry = ToolRegistry.get()
        registry.register(schema, fn_obj)
        if not enabled:
            registry.disable(tool_name)
        return fn

    if func is not None:
        return _register(func)
    return _register
