[<AutoOpen>]
module pilipala.util.text.markdown

open System
open fsharper.typ
open Markdig
open Markdig.Extensions

type Markdown = { markdown: string }

type Markdown with
    /// 将markdown字符串转换为html字符串
    member self.intoHtml() =

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

        { html = Markdown.ToHtml(self.markdown, pipeline) }
