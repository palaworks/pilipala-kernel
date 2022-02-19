module pilipala.util.markdown

open System
open fsharper.fn
open fsharper.op
open fsharper.ethType
open fsharper.typeExt
open fsharper.moreType
open Markdig
open Markdig.Extensions

type String with

    /// 将markdown字符串转换为html字符串
    member self.markdownInHtml =

        let exts: IMarkdownExtension list =
            [ Tables.PipeTableExtension() //表格解析
              EmphasisExtras.EmphasisExtraExtension() //额外的强调
              Mathematics.MathExtension() //LaTeX解析
              TaskLists.TaskListExtension() ] //任务列表

        let builder =
            foldl
                (fun (builder: MarkdownPipelineBuilder) ext ->
                    builder.Extensions.Add ext
                    builder)
                (MarkdownPipelineBuilder())
                exts

        let pipeline = builder.Build()

        Markdown.ToHtml(self, pipeline) //序列化到Json
