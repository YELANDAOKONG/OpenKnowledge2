# OpenKnowledge2
[C#/Study] OpenKnowledge System, Open-Sources for everyone! 

---

# 关于此项目

OpenKnowledge 是一个完全开源的知识学习框架协议规范。

此仓库存放的是 OpenKnowledge 的 C# 实现。

由于暂时没有详细的规范文档来规范 Exam 数据格式，
目前请参考 C# 代码的数据结构来了解相关数据结构。

**当前代码实现并不是最完美的解决方案，请等待后续优化。**

[协议规范文档](PROTOCOL.MD)

> **示例试卷格式 (JSON)**

示例没有涵盖所有支持的题目类型；
如果需要了解详细结构，请参考 C# 命名空间 `LibraryOpenKnowledge.Models` 下的文件。

```json
{
  "ExaminationVersion": {
    "Major": 1,
    "Minor": 0,
    "Patch": 0
  },
  "ExaminationMetadata": {
    "ExamId": "TEST-ID",
    "Title": "TEST-TITLE",
    "Description": "TEST-DESCRIPTION",
    "Subject": "TEST-SUBJECT",
    "Language": "zh-CN",
    "TotalScore": 100.0,
    "ReferenceMaterials": []
  },
  "ExaminationSections": [
    {
      "SectionId": "TEST-SECTION-ID",
      "Title": "TEST-SECTION-TITLE",
      "Description": "TEST-SECTION-DESCRIPTION",
      "ReferenceMaterials": [
        {
          "Materials": [
            "TEST-MATERIAL-1",
            "TEST-MATERIAL-2"
          ],
          "Images": null
        }
      ],
      "Score": 100.0,
      "Questions": [
        {
          "QuestionId": "TEST-QUESTION-ID",
          "Type": 1,
          "Stem": "1 and 100 which is bigger?",
          "Options": [
            {
              "Item1": "A",
              "Item2": "1"
            },
            {
              "Item1": "B",
              "Item2": "100"
            }
          ],
          "Score": 10.0,
          "UserAnswer": null,
          "Answer": [
            "A"
          ],
          "ReferenceAnswer": null,
          "ReferenceMaterials": [],
          "IsAiJudge": false,
          "Commits": []
        },
        {
          "QuestionId": "TEST-QUESTION-ID-2",
          "Type": 1,
          "Stem": "1000 and 100 which is bigger?",
          "Options": [
            {
              "Item1": "A",
              "Item2": "1000"
            },
            {
              "Item1": "B",
              "Item2": "100"
            }
          ],
          "Score": 80.0,
          "UserAnswer": null,
          "Answer": [
            "B"
          ],
          "ReferenceAnswer": null,
          "ReferenceMaterials": [],
          "IsAiJudge": false,
          "Commits": []
        },
        {
          "QuestionId": "TEST-QUESTION-ID-3",
          "Type": 5,
          "Stem": "1 + 114514 + 2 * 3 = (?), with calculation processing",
          "Options": [],
          "Score": 100.0,
          "UserAnswer": null,
          "Answer": [
            "114521"
          ],
          "ReferenceAnswer": null,
          "ReferenceMaterials": [],
          "IsAiJudge": true,
          "Commits": [
            "1 + 114514 + 2 * 3 = 1 + 114514 + 6",
            "1 + 114514 + 2 * 3 = 114515 + 6",
            "1 + 114514 + 2 * 3 = 114521"
          ]
        }
      ]
    }
  ]
}
```

