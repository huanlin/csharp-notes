# File-scoped 命名空間

C# 10 新增了 file-scoped namespace 語法，中文或者譯作「檔案範圍的命名空間宣告」，但我覺得沒有英文來得直觀明瞭，故往後提及此語法時會使用原文 file-scoped。

File-scoped 命名空間宣告只要寫一行，就可以讓整個檔案裡面的型別全都隸屬在指定的命名空間裡。光用白話解釋恐怕不好理解，看程式碼會更清楚。原本我們都是以一對大括號來界定一個命名空間的有效範圍，例如：

~~~~~~~~csharp
namespace Models
{
    class Employee {  }
    class Customer {  }
}
~~~~~~~~

在 C# 10，可以這樣寫：

~~~~~~~~csharp
namespace Models;  // 宣告整個檔案範圍的命名空間

class Employee {  }  
class Customer {  }
~~~~~~~~

顯而易見，這個 file-scoped 命名空間語法的好處是可以讓我們的程式碼減少一層縮排。

> Visual Studio 編輯器的預設值是「Block scoped」。你可以為專案（solution）加入一個 EditorConfig 檔案，並將其中的「namespace declarations」設定改為「File scoped」。如此一來，以後在此專案中新加入的 C# 檔案，其命名空間就會是 file-scoped 寫法。設定 EditorConfig 檔案的操作步驟可參考這個 Youtube 短片：https://youtu.be/AZ8aZzZYrXA。
