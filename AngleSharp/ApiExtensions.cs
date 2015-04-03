﻿namespace AngleSharp
{
    using AngleSharp.Dom;
    using AngleSharp.Dom.Css;
    using AngleSharp.Dom.Events;
    using AngleSharp.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A set of useful extension methods when dealing with the DOM.
    /// </summary>
    public static class ApiExtensions
    {
        #region Generic extensions

        /// <summary>
        /// Creates an element of the given type or returns null, if there is
        /// no such type.
        /// </summary>
        /// <typeparam name="TElement">The type of element to create.</typeparam>
        /// <param name="document">The responsible document.</param>
        /// <returns>The new element, if available.</returns>
        public static TElement CreateElement<TElement>(this IDocument document)
            where TElement : IElement
        {
            if (document == null)
                throw new ArgumentNullException("document");

            var type = typeof(ApiExtensions).GetAssembly().GetTypes()
                .Where(m => m.Implements<TElement>())
                .FirstOrDefault(m => !m.IsAbstractClass());

            if (type == null)
                return default(TElement);

            var ctor = type.GetConstructor();
            var parameterLess = ctor != null;
            
            if (parameterLess == false)
                ctor = type.GetConstructor(new Type[] { typeof(Document) });

            if (ctor == null)
                return default(TElement);

            var element = (TElement)(parameterLess ? ctor.Invoke(null) : ctor.Invoke(new Object[] { document }));
            var el = element as Element;

            if (element != null)
                document.Adopt(element);

            return element;
        }

        /// <summary>
        /// Creates a new DocumentFragment from the given HTML code. The
        /// fragment is parsed with the Body element as context.
        /// </summary>
        /// <param name="document">The responsible document.</param>
        /// <param name="html">The HTML to transform into a fragment.</param>
        /// <returns>The fragment containing the new nodes.</returns>
        static IDocumentFragment CreateFromHtml(this IDocument document, String html)
        {
            return new DocumentFragment(document.Body as Element, html);
        }

        /// <summary>
        /// Returns a task that is completed once the event is fired.
        /// </summary>
        /// <typeparam name="TEventTarget">The event target type.</typeparam>
        /// <param name="node">The node that fires the event.</param>
        /// <param name="eventName">The name of the event to be awaited.</param>
        /// <returns>The awaitable task returning the event arguments.</returns>
        public static async Task<Event> AwaitEvent<TEventTarget>(this TEventTarget node, String eventName)
            where TEventTarget : IEventTarget
        {
            if (node == null)
                throw new ArgumentNullException("node");
            else if (eventName == null)
                throw new ArgumentNullException("eventName");

            var completion = new TaskCompletionSource<Event>();
            DomEventHandler handler = (s, ev) => completion.TrySetResult(ev);
            node.AddEventListener(eventName, handler);

            try { return await completion.Task; }
            finally { node.RemoveEventListener(eventName, handler); }
        }

        /// <summary>
        /// Inserts a node as the last child node of this element.
        /// </summary>
        /// <typeparam name="TElement">The type of element to add.</typeparam>
        /// <param name="parent">The parent of the node to add.</param>
        /// <param name="element">The element to be appended.</param>
        /// <returns>The appended element.</returns>
        public static TElement AppendElement<TElement>(this INode parent, TElement element)
            where TElement : class, IElement
        {
            return parent.AppendChild(element) as TElement;
        }

        /// <summary>
        /// Inserts the newElement immediately before the referenceElement.
        /// </summary>
        /// <typeparam name="TElement">The type of element to add.</typeparam>
        /// <param name="parent">The parent of the node to add.</param>
        /// <param name="newElement">The node to be inserted.</param>
        /// <param name="referenceElement">
        /// The existing child element that will succeed the new element.
        /// </param>
        /// <returns>The inserted element.</returns>
        public static TElement InsertElement<TElement>(this INode parent, TElement newElement, INode referenceElement)
            where TElement : class, IElement
        {
            return parent.InsertBefore(newElement, referenceElement) as TElement;
        }

        /// <summary>
        /// Removes a child node from the current element, which must be a
        /// child of the current node.
        /// </summary>
        /// <typeparam name="TElement">The type of element.</typeparam>
        /// <param name="parent">The parent of the node to remove.</param>
        /// <param name="element">The element to be removed.</param>
        /// <returns>The removed element.</returns>
        public static TElement RemoveElement<TElement>(this INode parent, TElement element)
            where TElement : class, IElement
        {
            return parent.RemoveChild(element) as TElement;
        }

        /// <summary>
        /// Returns the first element matching the selectors with the provided
        /// type, or null.
        /// </summary>
        /// <typeparam name="TElement">The type to look for.</typeparam>
        /// <param name="parent">The parent of the nodes to gather.</param>
        /// <param name="selectors">The group of selectors to use.</param>
        /// <returns>The element, if there is any.</returns>
        public static TElement QuerySelector<TElement>(this IParentNode parent, String selectors)
            where TElement : class, IElement
        {
            return parent.QuerySelector(selectors) as TElement;
        }

        /// <summary>
        /// Returns a list of elements matching the selectors with the
        /// provided type.
        /// </summary>
        /// <typeparam name="TElement">The type to look for.</typeparam>
        /// <param name="parent">The parent of the nodes to gather.</param>
        /// <param name="selectors">The group of selectors to use.</param>
        /// <returns>An enumeration with the elements.</returns>
        public static IEnumerable<TElement> QuerySelectorAll<TElement>(this IParentNode parent, String selectors)
            where TElement : IElement
        {
            return parent.QuerySelectorAll(selectors).OfType<TElement>();
        }

        /// <summary>
        /// Gets the descendent nodes of the given parent.
        /// </summary>
        /// <typeparam name="TNode">The type of nodes to obtain.</typeparam>
        /// <param name="parent">The parent of the nodes to gather.</param>
        /// <returns>The descendent nodes.</returns>
        public static IEnumerable<TNode> Descendents<TNode>(this INode parent)
        {
            return parent.Descendents().OfType<TNode>();
        }

        /// <summary>
        /// Gets the descendent nodes of the given parent.
        /// </summary>
        /// <param name="parent">The parent of the nodes to gather.</param>
        /// <returns>The descendent nodes.</returns>
        public static IEnumerable<INode> Descendents(this INode parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            return parent.GetDescendants();
        }

        /// <summary>
        /// Gets the ancestor nodes of the given child.
        /// </summary>
        /// <typeparam name="TNode">The type of nodes to obtain.</typeparam>
        /// <param name="child">The child of the nodes to gather.</param>
        /// <returns>The ancestor nodes.</returns>
        public static IEnumerable<TNode> Ancestors<TNode>(this INode child)
        {
            return child.Ancestors().OfType<TNode>();
        }

        /// <summary>
        /// Gets the ancestor nodes of the given child.
        /// </summary>
        /// <param name="child">The child of the nodes to gather.</param>
        /// <returns>The ancestor nodes.</returns>
        public static IEnumerable<INode> Ancestors(this INode child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            return child.GetAncestors();
        }

        #endregion

        #region Construction helpers

        /// <summary>
        /// Interprets the string as HTML source code and returns new
        /// HTMLDocument with the DOM representation.
        /// </summary>
        /// <param name="content">The string to use as source code.</param>
        /// <param name="configuration">
        /// [Optional] Custom options to use for the document generation.
        /// </param>
        /// <returns>The HTML document.</returns>
        public static IDocument ParseHtml(this String content, IConfiguration configuration = null)
        {
            return DocumentBuilder.Html(content, configuration);
        }

        /// <summary>
        /// Interprets the string as CSS source code and returns new
        /// CSSStyleSheet with the CSS-OM representation.
        /// </summary>
        /// <param name="content">The string to use as source code.</param>
        /// <param name="configuration">
        /// [Optional] Custom options to use for the document generation.
        /// </param>
        /// <returns>The CSS stylesheet.</returns>
        public static ICssStyleSheet ParseCss(this String content, IConfiguration configuration = null)
        {
            return DocumentBuilder.Css(content, configuration);
        }

        #endregion
    }
}
