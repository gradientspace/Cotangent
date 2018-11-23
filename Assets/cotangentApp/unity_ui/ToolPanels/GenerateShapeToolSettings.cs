#pragma warning disable 414
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;


public class GenerateShapeToolSettings : BaseToolSettings<GenerateShapeTool>
{
    MappedDropDown shapeTypeMode;

    InputField shapeHeight;
    InputField shapeWidth;
    InputField shapeDepth;
    InputField subdivisions;

    protected override void register_parameters()
    {
        shapeTypeMode = base.RegisterDropDown("ShapeTypeDropDown", "shape_type",
            new List<string>() {
                "Box", "Sphere", "Cylinder", "Arrow", "Bunny" },
            new List<int>() {
                (int)GenerateShapeTool.ShapeTypes.Box,
                (int)GenerateShapeTool.ShapeTypes.Sphere,
                (int)GenerateShapeTool.ShapeTypes.Cylinder,
                (int)GenerateShapeTool.ShapeTypes.Arrow,
                (int)GenerateShapeTool.ShapeTypes.Bunny
            });

        subdivisions = base.RegisterIntInput("SubdivisionsInput", "subdivisions", new Interval1i(1, 1000));

        shapeHeight = base.RegisterFloatInput("HeightInput", "shape_height", new Interval1d(0, 1000000));
        shapeWidth = base.RegisterFloatInput("WidthInput", "shape_width", new Interval1d(0, 1000000));
        shapeDepth = base.RegisterFloatInput("DepthInput", "shape_depth", new Interval1d(0, 1000000));
    }

    protected override void after_tool_values_update()
    {
        GenerateShapeTool.ShapeTypes type = Tool.ShapeType;
        switch (type) {
            case GenerateShapeTool.ShapeTypes.Box:
                shapeHeight.interactable = shapeWidth.interactable = shapeDepth.interactable = true; break;

            case GenerateShapeTool.ShapeTypes.Bunny:
            case GenerateShapeTool.ShapeTypes.Sphere:
                shapeHeight.interactable = true; shapeWidth.interactable = false; shapeDepth.interactable = false; break;

            case GenerateShapeTool.ShapeTypes.Cylinder:
            case GenerateShapeTool.ShapeTypes.Arrow:
                shapeHeight.interactable = shapeWidth.interactable = true; shapeDepth.interactable = false; break;
        }

        subdivisions.interactable = (type != GenerateShapeTool.ShapeTypes.Bunny);
    }








}

