---

# OpenKnowledge 考试系统数据交换协议规范

---

## 0. 协议版本

主版本号：`2`

次版本号：`1`

修订代号：`0`

> 当前版本：`2.1.0`

---

## 1. 协议概述

OpenKnowledge 是一种标准化的考试数据交换协议，用于定义考试内容、题目和评分的数据结构。本规范详细定义了协议的所有数据类型，确保不同实现的互操作性和一致性。

---

## 2. 核心数据结构

### 2.1 考试 (Examination)

考试是整个协议的根对象，包含版本信息、元数据和章节集合。

```csharp
[Serializable]
public class Examination
{
    public ExaminationVersion ExaminationVersion { get; set; } = new();
    public ExaminationMetadata ExaminationMetadata { get; set; } = new();
    public ExaminationSection[] ExaminationSections { get; set; } = new ExaminationSection[] { };
}
```

### 2.2 版本号 (ExaminationVersion)

采用语义化版本控制，由主版本、次版本和修订号组成。

```csharp
[Serializable]
public class ExaminationVersion : ISerializable
{
    public int Major { get; set; } = 0;
    public int Minor { get; set; } = 0;
    public int Patch { get; set; } = 0;
  
    public ExaminationVersion() { }
  
    public ExaminationVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    protected ExaminationVersion(SerializationInfo info, StreamingContext context)
    {
        Major = info.GetInt32("Major");
        Minor = info.GetInt32("Minor");
        Patch = info.GetInt32("Patch");
    }
  
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Major", Major);
        info.AddValue("Minor", Minor);
        info.AddValue("Patch", Patch);
    }
}
```

### 2.3 考试元数据 (ExaminationMetadata)

包含考试的基本信息，如标题、描述、学科等。

```csharp
[Serializable]
public class ExaminationMetadata
{
    public string? ExamId { get; set; } = null;
    public string Title { get; set; } = "Default";
    public string? Description { get; set; } = null;
    public string? Subject { get; set; } = null;
    public string? Language { get; set; } = null;
    public double TotalScore { get; set; } = 0;
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { };
}
```

### 2.4 考试章节 (ExaminationSection)

章节是考试的组织单元，包含一组相关题目和章节特定的参考资料。

```csharp
[Serializable]
public class ExaminationSection
{
    public string? SectionId { get; set; } = null;
    public string Title { get; set; } = "Default";
    public string? Description { get; set; } = null;
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { };
    public double? Score { get; set; } = null;
    public Question[]? Questions { get; set; } = new Question[] { };
}
```

### 2.5 题目 (Question)

题目是考试的基本组成单元，支持多种题型，包括选择题、判断题、填空题等。

```csharp
[Serializable]
public class Question
{
    public string? QuestionId { get; set; } = null;
    public QuestionTypes Type { get; set; } = QuestionTypes.Unknown;
    public string Stem { get; set; } = "Default";
    public List<Option> Options { get; set; } = new List<Option>(); // public List<(string, string)>? Options { get; set; } = new List<(string, string)>();
    public double Score { get; set; } = 1.0;

    public string[]? UserAnswer { get; set; }
    public string[] Answer { get; set; }
    public string[]? ReferenceAnswer { get; set; }
  
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { };
  
    public bool IsAiJudge { get; set; } = false;
    public string[]? Commits { get; set; } = new string[] { };
  
    public Question[]? SubQuestions { get; set; } = new Question[] { };
}
```

### 2.6 题目类型枚举 (QuestionTypes)

定义支持的题目类型，便于自动化处理和评分。

```csharp
public enum QuestionTypes
{
    Unknown = 0,
    SingleChoice = 1,
    MultipleChoice = 2,
    Judgment = 3,
    FillInTheBlank = 4,
    Math = 5,
    Essay = 6,
    ShortAnswer = 7,
    Calculation = 8,
    Complex = 9,
    Other = 10,
}
```

### 2.7 参考资料 (ReferenceMaterial)

为考试、章节或题目提供参考材料，可包含文本和图像。

```csharp
[Serializable]
public class ReferenceMaterial
{
    public string[] Materials { get; set; } = new string[] { };
    public ReferenceMaterialImage[]? Images { get; set; } = null;
}
```

### 2.8 参考图片 (ReferenceMaterialImage)

定义参考资料中的图片格式和来源。

```csharp
[Serializable]
public class ReferenceMaterialImage
{
    public ReferenceMaterialImageTypes Type { get; set; } = ReferenceMaterialImageTypes.Unknown;
    public string? Uri { get; set; } = null;
    public byte[]? Image { get; set; } = null;
}
```

### 2.9 图片类型枚举 (ReferenceMaterialImageTypes)

定义图片资源的类型和来源。

```csharp
[Serializable]
public enum ReferenceMaterialImageTypes
{
    Unknown = 0,
    Local = 1,
    Remote = 2,
    Embedded = 3,
}
```

### 2.10 题目评分 (QuestionScore)

记录每个题目的评分情况。

```csharp
[Serializable]
public class QuestionScore
{
    public string QuestionId { get; set; } = string.Empty;
    public double MaxScore { get; set; } = 0;
    public double ObtainedScore { get; set; } = 0;
    public bool IsCorrect { get; set; } = false;
}
```

### 2.11 成绩记录 (ScoreRecord)

记录用户完成考试的成绩和详细评分情况。

```csharp
[Serializable]
public class ScoreRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public double TotalScore { get; set; } = 0;
    public double ObtainedScore { get; set; } = 0;
    public Dictionary<string, double> SectionScores { get; set; } = new();
    public Dictionary<string, Dictionary<string, QuestionScore>> QuestionScores { get; set; } = new();
}
```

## 2.12 选项 (Option)

选项是题目的可选答案，用于选择题等题型，提供更结构化的方式替代之前的元组列表格式。

```csharp
[Serializable]
public class Option
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
```

---

## 3. 数据字段详细规范

### 3.1 ExaminationVersion

| 字段 | 类型 | 必需 |                 描述                 |
| :-----: | :----: | :----: | :------------------------------------: |
| Major | int |  是  |      主版本号，不兼容变更时递增      |
| Minor | int |  是  | 次版本号，向后兼容的功能性变更时递增 |
| Patch | int |  是  |   修订号，向后兼容的问题修复时递增   |

### 3.2 ExaminationMetadata

|        字段        |        类型        | 必需 |                  描述                  |
| :------------------: | :-------------------: | :----: | :--------------------------------------: |
|       ExamId       |       string       |  否  | 考试唯一标识符，建议使用UUID或自定义ID |
|       Title       |       string       |  是  |      考试标题，默认值为"Default"      |
|    Description    |       string       |  否  |              考试详细描述              |
|      Subject      |       string       |  否  |             所属学科或领域             |
|      Language      |       string       |  否  |       语言代码 (如zh-CN, en-US)       |
|     TotalScore     |       double       |  是  |        考试总分值，必须为非负数        |
| ReferenceMaterials | ReferenceMaterial[] |  否  |      全局参考资料，适用于整个考试      |

### 3.3 ExaminationSection

|        字段        |        类型        | 必需 |                  描述                  |
| :------------------: | :-------------------: | :----: | :--------------------------------------: |
|     SectionId     |       string       |  否  | 章节唯一标识符，建议使用UUID或自定义ID |
|       Title       |       string       |  是  |      章节标题，默认值为"Default"      |
|    Description    |       string       |  否  |              章节详细描述              |
| ReferenceMaterials | ReferenceMaterial[] |  否  |           章节特定的参考资料           |
|       Score       |       double       |  否  |   章节分值，可为null表示使用题目总和   |
|     Questions     |     Question[]     |  是  |   题目集合，可以为空数组但不能为null   |

### 3.4 Question

|        字段        |        类型        | 必需 |                描述                |
| :------------------: | :-------------------: | :----: | :-----------------------------------: |
|     QuestionId     |       string       |  否  |           题目唯一标识符           |
|        Type        |    QuestionTypes    |  是  |           题目类型枚举值           |
|        Stem        |       string       |  是  |         题干文本，默认值为 `"Default"`         |
|      Options      |    List<Option>    |  否  | 选项列表，格式为 (选项ID, 选项内容) |
|       Score       |       double       |  是  |          题目分值，默认为 `1.0`          |
|     UserAnswer     |      string[]      |  否  |           用户提交的答案           |
|       Answer       |      string[]      |  是  |              标准答案              |
|  ReferenceAnswer  |      string[]      |  否  |           参考答案或解析           |
| ReferenceMaterials | ReferenceMaterial[] |  否  |         题目特定的参考资料         |
|     IsAiJudge     |        bool        |  是  |         是否需要AI辅助评分         |
|      Commits      |      string[]      |  否  |        AI评分提示或解题步骤        |
|    SubQuestions    |     Question[]     |  否  |      子题目集合，用于复合题型      |

### 3.5 ReferenceMaterial

|   字段   |           类型           | 必需 |            描述            |
| :---------: | :------------------------: | :----: | :--------------------------: |
| Materials |         string[]         |  是  | 文本材料集合，可以为空数组 |
|  Images  | ReferenceMaterialImage[] |  否  |   图片材料集合，可为null   |

### 3.6 ReferenceMaterialImage

| 字段 |            类型            | 必需 |                 描述                 |
| :-----: | :---------------------------: | :----: | :------------------------------------: |
| Type | ReferenceMaterialImageTypes |  是  |            图片类型枚举值            |
|  Uri  |           string           |  否  | 图片资源定位符，适用于远程和本地资源 |
| Image |           byte[]           |  否  |         嵌入式图片二进制数据         |

### 3.7 QuestionScore

|     字段     |  类型  | 必需 |      描述      |
| :-------------: | :------: | :----: | :--------------: |
|  QuestionId  | string |  是  | 题目唯一标识符 |
|   MaxScore   | double |  是  |   题目满分值   |
| ObtainedScore | double |  是  |    实际得分    |
|   IsCorrect   |  bool  |  是  |  是否完全正确  |

### 3.8 ScoreRecord

|      字段      |                         类型                         | 必需 |            描述            |
| :--------------: | :-----------------------------------------------------: | :----: | :--------------------------: |
|       Id       |                        string                        |  是  | 记录唯一标识符，默认为GUID |
|     ExamId     |                        string                        |  是  |         关联考试ID         |
|   ExamTitle   |                        string                        |  是  |          考试标题          |
|     UserId     |                        string                        |  是  |           用户ID           |
|    UserName    |                        string                        |  是  |           用户名           |
|   Timestamp   |                       DateTime                       |  是  | 记录时间戳，默认为当前时间 |
|   TotalScore   |                        double                        |  是  |          考试总分          |
| ObtainedScore |                        double                        |  是  |        实际获得总分        |
| SectionScores |              Dictionary<string, double>              |  是  |       章节得分映射表       |
| QuestionScores | Dictionary<string, Dictionary<string, QuestionScore>> |  是  |     题目得分嵌套映射表     |

---

## 4. 数据序列化规范

### 4.1 JSON序列化规则

- 属性名保持Pascal命名法（首字母大写）
- 枚举值序列化为数值
- 日期时间使用ISO 8601格式 (yyyy-MM-ddTHH:mm:ss.fffZ)
- 空数组序列化为 `[]`
- 可选字段为null时可省略

### 4.2 版本兼容性

- 实现必须忽略未知字段
- 解析旧版本数据时，新增字段使用默认值
- 格式版本号增加规则:

  - 主版本号：不兼容的数据结构变更
  - 次版本号：向后兼容的功能性变更
  - 修订号：格式修复或细节调整

---

## 5. 数据验证规则

实现必须验证以下规则:

- 版本号各部分必须为非负整数
- 分数值必须为非负数
- 必填字段不能为null或空字符串
- 数组类型不能为null，应使用空数组表示无数据
- 枚举值必须在定义范围内，否则使用Unknown值代替

---

## 6. 示例数据

```json
{
  "ExaminationVersion": {
    "Major": 2,
    "Minor": 1,
    "Patch": 0
  },
  "ExaminationMetadata": {
    "ExamId": "MATH-2023-001",
    "Title": "基础数学测验",
    "Description": "测试基本数学运算能力",
    "Subject": "数学",
    "Language": "zh-CN",
    "TotalScore": 100.0,
    "ReferenceMaterials": []
  },
  "ExaminationSections": [
    {
      "SectionId": "SECTION-1",
      "Title": "选择题部分",
      "Description": "请选择正确答案",
      "Score": 40.0,
      "Questions": [
        {
          "QuestionId": "Q1",
          "Type": 1,
          "Stem": "1 + 1 = ?",
          "Options": [
            {
              "Id": "A",
              "Text": "1"
            },
            {
              "Id": "B",
              "Text": "2"
            },
            {
              "Id": "C",
              "Text": "3"
            }
          ],
          "Score": 10.0,
          "Answer": [
            "B"
          ],
          "IsAiJudge": false,
          "Commits": []
        }
      ]
    }
  ]
}
```

---

## 7. 版本兼容性说明

### 7.1 版本 `2.1.0` 的主要变更

> 协议版本：`1.0.0` => `2.1.0`

主要变更点：

- 新增Option类替代之前的元组列表结构
- 添加双向隐式转换以确保与旧版本的兼容性
- 保留 `Item1` / `Item2` 属性以兼容之前使用元组解构的代码
- 添加OptionConverter支持JSON序列化时的格式控制

对于处理旧版本数据的实现者，需要注意：

- 旧版本使用元组格式的数据依然可以被正确解析
- 新的Option类提供了更丰富的功能和更清晰的数据结构
- 序列化时应使用OptionConverter以确保正确的JSON格式输出

处理新旧版本数据的转换代码示例：

```csharp
// 旧格式到新格式
List<(string, string)> oldOptions = GetOldFormatOptions();
List<Option> newOptions = oldOptions.Select(o => new Option(o.Item1, o.Item2)).ToList();

// 新格式到旧格式
List<Option> options = GetOptions();
List<(string, string)> tuples = options.Select(o => (o.Id, o.Text)).ToList();
```