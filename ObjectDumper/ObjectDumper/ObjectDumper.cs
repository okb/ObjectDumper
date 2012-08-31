using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;


namespace Bizi.Framework
{
    public class ObjectDumper
    {
        private const string css = "    <style type=\"text/css\" >"
                                   +
                                   @".domenabled #finderparent{border:1px solid #000;position:relative}.domenabled #finder{position:absolute;top:1em;left:1em}.domenabled ul#finder,.domenabled ul#finder li,.domenabled ul#finder ul{width:200px;list-style-type:none;margin:0;padding:0px}.domenabled ul#finder li{overflow:hidden;text-wrap:avoid;#fff-space:nowrap}.domenabled ul#finder ul.hidden{top:0px;left:-2000px;position:absolute}.domenabled ul#finder ul.shown{top:0px;left:240px;position:absolute}.domenabled #finder a.open{background:url(http://www.alistapart.com/d/complexdynamiclists/arrowon.gif) no-repeat 90% 50% #eee;padding-right:16px;padding-left:0px;display:block}.domenabled #finder a.parent{background:url(http://www.alistapart.com/d/complexdynamiclists/arrow.gif) no-repeat #fff 100% 50%;padding-right:16px;padding-left:0px}.domenabled ul#finder li a{color:#000;background:url(http://www.alistapart.com/d/complexdynamiclists/normal.gif) no-repeat #fff 0 50%;padding-left:16px;text-decoration:none}
	                                </style>";

        // TODO: Check if jquery is allready loaded before injection.
        private  string js =
            /*"<script type=\"text/javascript\" src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.8.0/jquery.min.js\"></script>\n"*/
             "<script type=\"text/javascript\">\n"
            + string.Join("\n", File.ReadAllLines(@"D:\gitrepos\ObjectDumper\ObjectDumper\ObjectDumper\js-src.js"))
            
            /*+ @"$(function () { var e = $('#finder'); if (!e) return; $('body').addClass('domenabled'); var t = 'parent', n = 'shown', r = 'hidden', i = 'open'; e.find('ul').addClass(r).find('ul:only-child').before('[item]'); var s = function () { var e = $(this), s = e.parent().parent(); return s.find('ul').each(function () { $(this).removeClass(n).addClass(r) }), s.find('a').each(function () { $(this).removeClass(i).addClass(t) }), e.removeClass(t).addClass(i), e.next('ul').addClass(n), !1 }; e.find('li:has(ul)').each(function () { var e = $(this).contents().filter(function () { return this.nodeName.toUpperCase() != 'UL' }), n = document.createElement('a'); n.href = '#', n.className = t, e.wrapAll(n).parent().click(s) }) })"*/
             +"   </script>";

        private int depth;
        protected int level;
        protected int pos;
        private TextWriter writer;


        public virtual void Dump(object element, int depth, TextWriter log)
        {
            this.depth = depth;
            this.writer = log;

            Write(css);
            Write(string.Format("<h1 title=\"{0}\">{1}</h1>", element.GetType(), element.GetType().Name));
            //Write("<div id=\"finderparent\">");
            //WriteObject(null, element);
            //Write("</div>");
            Write(js);
        }


        public void Dump(object element)
        {
            Dump(element, 0);
        }


        public void Dump(object element, int depth)
        {
            Dump(element, depth, Console.Out);
        }


        protected virtual bool WriteCustom(object o)
        {
            return false;
        }


        protected virtual void WriteIndent()
        {
            for (int i = 0; i < this.level; i++)
                this.writer.Write("  ");
        }


        protected virtual void WriteLine()
        {
            this.writer.WriteLine();
            this.pos = 0;
        }


        protected virtual void WriteString(string value)
        {
            Write(value);
        }


        protected virtual void WriteTab()
        {
            Write("  ");
            while (this.pos % 8 != 0)
                Write(" ");
        }


        protected virtual void WriteValue(object o)
        {
            if (o == null)
                Write("null");
            else if (WriteCustom(o))
                return;
            else if (o is String)
                WriteString((string)o);
            else if (o is DateTime)
                Write(((DateTime)o).ToShortDateString());
            else if (o is ValueType)
                Write(o.ToString());
            else if (o is IEnumerable)
                Write("...");
            else
                Write("{ }");
        }


        protected void Write(string s)
        {
            if (s == null)
                return;

            this.writer.Write(s);
            this.pos += s.Length;
        }


        private void WriteObject(string prefix, object element)
        {
            if (element is Type && element.GetType().ToString() == "System.RuntimeType")
            {
                WriteIndent();
                Write(prefix);
                WriteValue(element.ToString());
                WriteLine();
                return;
            }

            if (element == null || element is ValueType || element is string || element is Uri)
            {
                WriteIndent();
                Write(prefix);
                WriteValue(element);
                WriteLine();
                return;
            }
            
            Write(this.level == 0 ? "<ul id=\"finder\">" : "<ul>");
            MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
            WriteIndent();
            Write(prefix);
            bool propWritten = false;
            foreach (MemberInfo m in members)
            {
                FieldInfo f = m as FieldInfo;
                PropertyInfo p = m as PropertyInfo;
                bool fieldNotNull = f != null;
                if (!fieldNotNull && p == null)
                    continue;

                if (propWritten)
                    WriteTab();
                else
                    propWritten = true;

                Type t = fieldNotNull ? f.FieldType : p.PropertyType;

                Write("<li>");
                Write(string.Format("<strong title=\"{1}\">{0}</strong>", m.Name, t));
                Write(": ");

                object value = null;
                if (f != null)
                    value = f.GetValue(element);
                else
                {
                    ParameterInfo[] indexParameters = p.GetIndexParameters();
                    int IndexerCount = indexParameters.Count();
                    if (IndexerCount > 0)
                    {
                        IDictionary<string, object> dictionaryElement = element as IDictionary<string, object>;
                        IEnumerable enumerableElement = element as IEnumerable;
                        if(dictionaryElement != null)
                            Dictionary(prefix, dictionaryElement);
                        else if (enumerableElement != null)
                            Enumerable(prefix, enumerableElement);
                    }
                    else
                    {
                        try
                        {
                            value = p.GetValue(element, null);
                        }
                        catch (Exception)
                        {
                            value = "[Error getting value]";
                        }
                    }
                }
                if (t.IsValueType || t == typeof(string))
                    WriteValue(value);
                else
                {
                    if (this.level < this.depth)
                    {
                        this.level++;
                        WriteObject(null, value);
                        this.level--;
                    }
                }
                Write("</li>");
            }
            if (propWritten)
                WriteLine();

            /*if (this.level >= this.depth)
                return;

            foreach (MemberInfo m in members)
            {
                FieldInfo f = m as FieldInfo;
                PropertyInfo p = m as PropertyInfo;
                if (f == null && p == null)
                    continue;

                Type t = f != null ? f.FieldType : p.PropertyType;
                if ((t.IsValueType || t == typeof(string)))
                    continue;

                object value = f != null ? f.GetValue(element) : p.GetValue(element, null);

                if (value == null)
                    continue;

                this.level++;
                WriteObject("<h3>"+ m.Name + "</h3><ul>: ", value);
                Write("</li>");
                this.level--;
            }*/
            Write("</ul>");
        }


        private void Enumerable(string prefix, IEnumerable enumerableElement)
        {
            if (enumerableElement == null)
                return;

            var countElements = enumerableElement.Cast<object>().Count();
            if (countElements == 0)
            {
                WriteIndent();
                Write(prefix);
                Write("Count = 0");
                WriteLine();
                return;
            }
            Write("<ul>");
            foreach (object item in enumerableElement)
            {
                Write("<li>");
                if (item is IEnumerable && !(item is string))
                {
                    WriteIndent();
                    Write(prefix);
                    Write("...");
                    WriteLine();

                    if (this.level < this.depth)
                    {
                        this.level++;
                        WriteObject(prefix, item);
                        this.level--;
                    }
                }
                else
                    WriteObject(prefix, item);

                Write("</li>");
            }
            Write("</ul>");
            return;
        }


        private void Dictionary(string prefix, IDictionary<string, object> dictionaryElement)
        {
            if (dictionaryElement == null)
                return;
            
            var countElements = dictionaryElement.Cast<object>().Count();
            if (countElements == 0)
            {
                WriteIndent();
                Write(prefix);
                Write("[Count = 0]");
                WriteLine();
                return;
            }
            Write("<ul>");
            foreach (var item in dictionaryElement)
            {
                Write("<li>");
                WriteObject(prefix, item.Key);
                Write(": ");
                if ((item.Value is IEnumerable) && !(item.Value is string))
                {
                    WriteIndent();
                    Write(prefix);
                    Write("...");
                    WriteLine();
                    if (this.level < this.depth)
                    {
                        this.level++;
                        WriteObject(prefix, item.Value);
                        this.level--;
                    }
                }
                else
                    WriteObject(prefix, item.Value);
                Write("</li>");
            }
            Write("</ul>");
        }
    }

    public class HtmlObjectDumper : ObjectDumper
    {
        protected override bool WriteCustom(object o)
        {
            if (o is bool)
            {
                Write(string.Format("<input type=\"checkbox\" disabled=\"disabled\"{0}>",
                                    ((bool)o) == true ? " checked=\"checked\"" : ""));
                return true;
            }
            Uri u = o as Uri;
            if (u != null)
            {
                Write(string.Format("<a href=\"{0}\">{0}</a>", u.OriginalString));
                return true;
            }
            return false;
        }


        protected override void WriteIndent()
        {
            /*for (int i = 0; i < this.level; i++)
                Write("<span>...</span>");*/
        }


        protected override void WriteLine()
        {
            //Write("</br>");
            this.pos = 0;
        }


        protected override void WriteString(string value)
        {
            Write(HttpUtility.HtmlEncode(value));
        }


        protected override void WriteTab()
        {
            //Write("<span>,,,</span>");
        }
    }

    public static class ObjectExtensions
    {
        public static void Dump(this Object value)
        {
            new ObjectDumper().Dump(value);
        }


        public static void Dump(this Object value, int depth)
        {
            new ObjectDumper().Dump(value, depth);
        }


        public static void Dump(this Object value, int depth, TextWriter writer)
        {
            new ObjectDumper().Dump(value, depth, writer);
        }


        public static string DumpAsHtml(this Object value)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                new HtmlObjectDumper().Dump(value, 0, stringWriter);
                return stringWriter.ToString();
            }
        }


        public static string DumpAsHtml<T>(this T value, int depth)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                new HtmlObjectDumper().Dump(value, depth, stringWriter);
                return stringWriter.ToString();
            }
        }


        public static void DumpAsHtml(this Object value, int depth, TextWriter writer)
        {
            new HtmlObjectDumper().Dump(value, depth, writer);
        }
    }
}
