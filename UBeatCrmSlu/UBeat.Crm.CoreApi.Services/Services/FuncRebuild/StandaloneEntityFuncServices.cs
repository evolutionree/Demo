using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Services.FuncRebuild
{

    /// <summary>
    /// 独立实体的func结构包括：
    /// 1、主节点（实体节点），移动端和WEB端各一个，共两个，这个必须有
    /// 2、二级节点包括：功能、菜单、主页动态、主页Tab，四组共8个，这个也是必须有的。
    /// 3、主页动态下来源为动态来源，来源包括：a、关联了主实体的动态实体(包括动态实体有流程的），b、独立实体的启用的流程
    /// 4、菜单下的来源是实体配置中的快速筛选项
    /// 5、功能下包括：a、新增、修改、删除、转移、打印、导出、导入、禁用、启用等（逐步切换到funcbutton上），b、funcbutton中定义的button
    /// 6、主页Tab来源于关联页签，其中文档包含了“文档删除”的子功能
    /// </summary>
    public class StandaloneEntityFuncServices : BasicBaseServices
    {
        public StandaloneEntityFuncServices() {

        }
        public void RebuildFuncs(Guid entityId) {

        }
    }
}
