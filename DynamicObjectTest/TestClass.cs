using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DynamicObjectTest
{
    public class TestClass
    {
        public static void TestSampleObject()
        {
            dynamic obj = new SampleObject();
            Console.WriteLine(obj.SampleProperty);

        }

        public static void TestDynamicXMLNode()
        {
            dynamic contact = new DynamicXMLNode("Contacts");
            contact.Name = "Patrick Hines";
            contact.Phone = "206 - 555 - 0144";
            contact.Address = new DynamicXMLNode();
            contact.Address.Street = "123 Main St";
            contact.Address.City = "Mercer Island";
            contact.Address.State = "WA";
            contact.Address.Postal = "68402";

            string state = contact.Address.State;//需要重写TryConvert方法，直接转型成string会出异常
            Console.WriteLine(state);
            var toStr = contact.ToString();
            Console.WriteLine(toStr);
        }
    }

    public class SampleObject : DynamicObject
    {
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = binder.Name;
            return true;
        }
    }

    /// <summary>
    /// DynamicXMLNode
    /// </summary>
    /// <remarks>对 XElement node 的包装</remarks>
    public class DynamicXMLNode : DynamicObject
    {
        XElement node;

        public DynamicXMLNode(XElement node)
        {
            this.node = node;
        }

        public DynamicXMLNode()
        {
        }

        public DynamicXMLNode(String name)
        {
            node = new XElement(name);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            XElement setNode = node.Element(binder.Name);
            if (setNode != null)//已有元素
            {
                setNode.SetValue(value);
            }
            else//新元素
            {
                node.Add(value.GetType() == typeof(DynamicXMLNode)
                    ? new XElement(binder.Name)//非叶子节点，例子中的Address
                    : new XElement(binder.Name, value));//叶子节点
            }
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            XElement getNode = node.Element(binder.Name);
            if (getNode != null)
            {
                result = new DynamicXMLNode(getNode);
                return true;
            }
            result = null;
            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(string))
            {
                result = node.Value;
                return true;
            }
            result = null;
            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type xmlType = typeof(XElement);
            try
            {
                result = xmlType.InvokeMember(binder.Name,
                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, node, args);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
