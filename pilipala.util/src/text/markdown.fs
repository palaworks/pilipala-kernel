﻿module pilipala.util.markdown

open System
open fsharper.typ
open Markdig
open Markdig.Extensions
open pilipala.util.html

type Markdown = { value: string }

type Markdown with

    /// 将markdown字符串转换为html字符串
    member self.intoHtml =

        let exts: IMarkdownExtension list =
            [ Tables.PipeTableExtension() //表格解析
              EmphasisExtras.EmphasisExtraExtension() //额外的强调
              Mathematics.MathExtension() //LaTeX解析
              TaskLists.TaskListExtension() ] //任务列表

        let builder =
            foldl
            <| fun (builder: MarkdownPipelineBuilder) ext ->
                builder.Extensions.Add ext
                builder
            <| MarkdownPipelineBuilder()
            <| exts

        let pipeline = builder.Build()

        { value = Markdown.ToHtml(self.value, pipeline) }: Html
