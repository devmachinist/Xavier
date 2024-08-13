using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;

namespace Xavier
{
    public class Memory
    {
        public List<object> XavierNodes { get; set; } = new List<object>();
        public string? XavierName { get; set; } = "Xavier";
        public ScriptEngine PyEngine { get; set; }
        public string? BaseURI { get; set; }
        public string? JSModule { get; set; }
        public string? EFModule { get; set; }
        public string? StaticRoot { get; set; }
        public string? StaticFallback { get;set; }
        public List<DbContext>? Contexts { get; set; }
        public bool? AddAuthentication { get; set; } = true;
        public bool IsSPA {get;set;} = false;
        public string? JSAuth() => $@"";
        public Memory(){
            
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public async Task Init(string root, string destination, Assembly assembly, bool isSPA = false)
        {
            PyEngine = Python.CreateEngine();
            StaticRoot = root;
            XavierName = destination;
            IsSPA = isSPA;

            await Task.Run(async () =>
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder wb = new StringBuilder();
                sb.Append($@"{((IsSPA)? $@"document.addEventListener('click', function(event) {{
    if (event.target.tagName === 'A') {{
        // Prevent the default navigation behavior
        clearXidElements();
    }}
}});
window.onpopstate = function (event){{
    clearXidElements();
}}
function clearXidElements() {{
    var elementsWithXid = document.querySelectorAll('[xid]'); // Select all elements with xid attribute
    elementsWithXid.forEach(function(element) {{
        element.innerHTML = ''; // Clear the contents of each element
        element.removeAttribute('xid'); // Remove the xid attribute from each element
        if(element.tagName === 'SCRIPT'){{
            element.outerHTML = '';
        }}
    }});
}};
": $@"")}
    class ObservableArray extends Array {{
        constructor(...array) {{
            super(...array);
            this.subscribers = [];
        }}
        subscribe(callback) {{
            this.subscribers.push(callback);
        }}

        unsubscribe(callback) {{
            this.subscribers = this.subscribers.filter(subscriber => subscriber !== callback);
        }}

        notify(method, args, second,...items) {{
            this.subscribers.forEach(subscriber => {{
                subscriber.updateShadow(method,args,second,...items);
            }});
        }}

        push(...items) {{
            super.push(...items);
            this.notify('push',...items);
        }}

        pop() {{
            const item = super.pop();
            this.notify('pop');
            return item;
        }}

        shift() {{
            const item = super.shift();
            this.notify('shift');
            return item;
        }}

        unshift(...items) {{
            super.unshift(...items);
            this.notify('unshift',...items);
        }}

        splice(start, deleteCount, ...items) {{
            var result = super.splice(start, deleteCount, ...items);
            this.notify('splice', start, deleteCount, ...items);
            return result;
        }}
        reverse() {{
            super.reverse();
            this.notify('reverse');
        }}

        sort(compareFunction) {{
            super.sort(compareFunction);
            this.notify('sort', comparefunction);
        }}

        fill(value, start, end) {{
            super.fill(value, start, end);
            this.notify('fill', value,start,end);
        }}

        copyWithin(target, start, end) {{
            super.copyWithin(target, start, end);
            this.notify('copyWithin', target,start,end);
        }}

        map(callback) {{
            return super.map(callback);
        }}

        getAll() {{
            return this;
        }}
    }}
    class Watcher {{
        constructor(array, parentNode, xnode, segmentContent, onChange) {{
            this.array = array;
            this.idCounter = 0; // Initialize ID counter
            this.parentNode = parentNode;
            this.xnode = xnode;
            this.shadow = array.map((item, index) => this.createProxy(item, index)); // Initialize shadow array with correct indices, incremental IDs, and item copies
            this.onChange = onChange;
            this.segmentContent = segmentContent;
            this.firstRender = true;
            this.array.subscribe(this);
            // Create initial child nodes
            this.createChildNodes();
        }}
        update(items) {{
            // Update shadow array
            this.shadow = items.map((item, index) => this.createProxy(item, index)); // Initialize shadow array with correct indices, incremental IDs, and item copies

            // Notify onChange callback
            this.onChange(this.shadow);

            // Update child nodes
            this.updateChildNodes();
        }}
        createProxy(item, index) {{
            const formattedItem = {{ index: index, node: null, id: this.idCounter++, x: item, _changed: false }};
            return new Proxy(formattedItem, {{
                set: (target, prop, value) => {{
                    if (prop === 'x') {{
                        // Handle changes to the 'x' property
                        target[prop] = value;
                    }}
                        if (!this.firstRender) {{
                            this.updateChildNodes();
                        }}
                    return Reflect.set(target, prop, value);
                }}
            }});
        }}

        updateShadow(method, args, second,...items) {{
            if (method === 'splice') {{
                var deletedItems = this.shadow.splice(args, second, ...items);
                // Notify onChange callback
                this.onChange('splice', {{ args, second, items, deletedItems }});
                deletedItems.forEach((ditem) => {{
                    ditem.node.querySelectorAll('[xid]').forEach((element) => {{
                        var xid = element.getAttribute('xid');
                        element.querySelectorAll('[eachid]').forEach((each) => {{
                            var att = each.getAttribute('eachid');
                            delete window[att]
                        }})
                        delete window[xid];
                    }});
                    this.parentNode.removeChild(ditem.node);
                }})
                // Update child nodes
            }} else if (method === 'unshift') {{
                var items = [args];
                this.shadow.unshift(...items.map((item, index) => ({{ node: null,index: index, id: this.idCounter++, x: item, _changed: false }}))); // Update index, id, and item values based on new indices after unshift

                // Update index values for existing elements
                for (let i = items.length; i < this.shadow.length; i++) {{
                    this.shadow[i].index = i;
                }}

                // Notify onChange callback
                this.onChange('unshift', items);
                var x = this.shadow[0];
                var newNode = document.createElement('div');
                this.shadow[0].node = newNode;
                this.xnode.renderTemplate(this.xnode.parseTemplate(this.segmentContent), {{ ...this.xnode, ...x }}, this.shadow[0].node);

                // Update child nodes
                this.parentNode.insertBefore(this.shadow[0].node, this.parentNode.firstChild)
            }} else if (method === 'pop') {{
                const deletedItem = this.shadow.pop();

                // Notify onChange callback
                this.onChange('pop', deletedItem);
                deletedItem.node.querySelectorAll('[xid]').forEach((element) => {{
                    var xid = element.getAttribute('xid');
                    var parts = xid.split(""-"");
                    var count = parseInt(parts[parts.length - 1]);
                    delete window[xid];
                    count++
                    parts[parts.length - 1] = count
                    var nextxid = parts.join('-');
                    if (window[nextxid]) {{
                        if (window[nextxid].reordered) {{
                            delete window[nextxid].reordered
                            return deletedItem;
                        }}
                        window[nextxid].xid = xid;
                        window[nextxid].reordered = true;
                        window[nextxid].setAttribute('xid', xid);
                        window[nextxid].evalAttrs(window[nextxid].Element)
                        window[xid] = {{ ...window[nextxid] }}
                    }}
                    return deletedItem;
                }});



                // Remove last child node
                this.parentNode.lastChild.remove();
            }} else if (method === 'shift') {{
                const deletedItem = this.shadow.shift();

                // Update index values for remaining elements
                for (let i = 0; i < this.shadow.length; i++) {{
                    this.shadow[i].index = i;
                }}

                // Notify onChange callback
                this.onChange('shift', deletedItem);

                // Remove first child node
                this.parentNode.firstChild.remove();
            }} else if (method === 'push') {{
                const items = [args];
                const start = this.array.length - items.length;
                this.shadow.push(...items.map((item, index) => ({{ node: null, index: start + index, id: this.idCounter++, x: item , _changed: false }}))); // Update index, id, and item values based on new indices after push

                // Notify onChange callback
                this.onChange('push', items);

                // Create new child nodes
                this.createChildNodes(this.shadow.length - items.length);
            }} else {{
                this.shadow[method](args);

                // Handle other array methods if needed
            }}
        }}

        createChildNodes(startIndex = 0) {{
            const end = this.shadow.length;
            for (let i = startIndex; i < end; i++) {{
                var x = this.shadow[i];
                var newNode = document.createElement('div');
                this.shadow[i].node = newNode;
                this.parentNode.appendChild(this.shadow[i].node);
                this.xnode.renderTemplate(this.xnode.parseTemplate(this.segmentContent), {{ ...this.xnode, ...x }}, this.shadow[i].node);
            }}
            this.firstRender = false;
        }}
        

        updateChildNodes() {{
            this.shadow.forEach((item, index) => {{
                if (item._changed) {{
                    this.xnode.renderTemplate(this.xnode.parseTemplate(this.segmentContent), {{ ...this.xnode, ...item }}, item.node);
                    item._changed = false;
                }}
            }});
        }}
    }}
    class XavierNode{{
    constructor(){{
        this.ScriptRender = true;
        this.Element = {{}};
        this.InnerHTML = 'unset';
        this.MutationStack = [];
        this.LoadingVirtualDOM = false;
        this.Variables = [];
        this.VariableInfo = {{}};
    }}
    GetScripts(){{}};
    GetHTML(){{}};
    findLargestNodeDepth(node = document, currentDepth = 0) {{
        if (!node.hasChildNodes()) {{
            return currentDepth;
        }}
        else {{
            let maxDepth = currentDepth;
            const children = node.childNodes;
            for (let i = 0; i < children.length; i++) {{
                const child = children[i];
                if (child.nodeType === Node.ELEMENT_NODE) {{
                    const childDepth = this.findLargestNodeDepth(child, currentDepth + 1);
                    maxDepth = Math.max(maxDepth, childDepth);
                }}
            }}
            return maxDepth;
        }}
    }}
    async replaceElements() {{
            let attrs = this.evalAttrs(this.Element);
            this.setAttrs(attrs, this.index,this.Element);
            this.evalAttrs(this.Element);
            this.bindToWindow();
            let body = """";
            body = this.Element.innerHTML;
            while (this.Element.firstChild) this.Element.removeChild(this.Element.lastChild);
            // Add it to the InnerHTML property of the object
            this.InnerHTML = body;

            if (window.location.pathname === this.Route && this.ShouldRender === true || this.Route === '' && this.ShouldRender === true) {{
                this.Element.insertAdjacentHTML('afterbegin', this.GetHTML());
                this.Start();
            }}
            if (this.ScriptRender === true) {{
                var script = document.createElement('script');
                script.setAttribute('async', 'async');
                script.setAttribute('type', 'module');
                script.setAttribute('xid', `${{ this.xid}}`)
                if (window.location.pathname === this.Route && this.ScriptRender === true || this.Route === '' && this.ScriptRender === true) {{
                    script.insertAdjacentHTML('afterbegin', this.GetScripts());
                    document.body.append(script);
                    this.ScriptRender = false;
                }}
            }}
        }}

    async replaceVirtualElements() {{
        this.VirtualNode = this.Element.cloneNode(true);
            let attrs = this.evalAttrs(this.VirtualNode);
            this.setAttrs(attrs, this.index, this.VirtualNode);
            let body = """";
            while (this.VirtualNode.firstChild) this.VirtualNode.removeChild(this.VirtualNode.lastChild);
            // Add it to the InnerHTML property of the object

        this.VirtualNode.insertAdjacentHTML('afterbegin', this.RunTemplater(this.GetHTML()));
        this.MutateVirtual();
        }}
    evalAttrs(element){{
        let attrs = {{}};
        let attributes = element.attributes;
        for (let i=0 ; i<attributes.length ; i++){{
            let attr = attributes[i];
                this[attr.name] = attr.value;
                attrs[attr.name] = attr.value;
        }}
        return attrs;
    }}
    setAttrs(attrs, count, element){{
        for (let attrName in attrs){{
            element.setAttribute(attrName, attrs[attrName]);
        }}
        element.setAttribute('xid',`${{this.Xid}}-${{count.toString()}}`);            
    }}
    bindToWindow() {{
        let xid = this.Element.getAttribute('xid');
        this.Element.setAttribute('xid', xid);
        if (window[xid]) {{
            window[xid].Element = this.Element;
            window[xid].evalAttrs(window[xid].Element);
            window[xid].ScriptRender = true;
            window[xid].ShouldRender = true;
            var wx = window[xid];
            for (var key in this) {{
                this[key] = wx[key];
            }}
        }}        //this line sets the xid to the original for consistency
        else {{
            window[xid] = this; //this line instantiates the class with the given data
        }}
    }}
    Start() {{
        this.createBindings();
        this.replaceVariables();
        this.LoadingVirtualDOM = false;
        this.Variables = this.getVariables();
        const intervalId = setInterval(() => {{
            if(this.compareVarsToList().length > 0){{
                this.Update()
            }}
        }}, 50);
    }}
    Update(){{
        this.LoadingVirtualDOM = true;
        try{{
            this.OnUpdate();
            this.Variables = this.getVariables();
            this.updateVariables();
            this.LoadingVirtualDOM = false;
        }}catch(ex){{console.log(ex);}}
    }}
    OnUpdate(){{
    }}
    getElementByXid(xid){{
        if(xid){{
            return document.querySelector(""[xid='"" + xid + ""']""); 
        }}
        return document.querySelector(""[xid='"" + this.xid + ""']""); 
    }}
    RunTemplater(input,node) {{
    const template = this.parseTemplate(input);
    return this.renderTemplate(template, this, node);
    }}
    parseTemplate(templateString) {{
        const regex = /((-\[#)(if|each|switch)\s*(.*?)\]([\s\S]*)-\[\s*\/\s*\])|((-\[)([^#|\/].*?)\])/g;
            const segments = [];
            let lastIndex = 0;
            let match;

            while ((match = regex.exec(templateString)) !== null) {{
                const [fullMatch, fulldirective, directiveStart, directive, expression, content, fullvar, variableStart, variable] = match;
                const index = match.index;

                if (index !== lastIndex) {{
                    segments.push({{
                        type: 'text',
                        value: templateString.slice(lastIndex, index)
                    }});
                }}

                if (directiveStart) {{
                    segments.push({{
                        type: 'directive',
                        directive:""#""+directive,
                        expression: expression.trim(),
                        content: content.trim()
                    }});
                }} else if (variableStart) {{
                    segments.push({{
                        type: 'variable',
                        expression: variable.trim()
                    }});
                }}

                lastIndex = index + fullMatch.length;
            }}

            if (lastIndex < templateString.length) {{
                segments.push({{
                    type: 'text',
                    value: templateString.slice(lastIndex)
                }});
        }}

            return segments;
    }}
        renderTemplate(template, data, node) {{
            node.innerHTML = """";
        if (template) {{

            for (const segment of template) {{
                if (segment.type === 'text') {{
                    if (/[^ \t\r\n]+/.test(segment.value)) {{
                        var tmp = document.createElement('div');
                        tmp.innerHTML = segment.value;
                        node.appendChild(tmp)
                    }}
                }} else if (segment.type === 'directive') {{
                    if (segment.directive === '#if') {{
                        var condition = this.evaluateExpression(segment.directive + "" "" + segment.expression, data);
                        if (condition) {{
                            this.renderTemplate(this.parseTemplate(segment.content), data, node);
                        }}
                    }} else if (segment.directive === '#each') {{
                        var array = this.evaluateExpression(segment.directive + "" "" + segment.expression, data);
                        var xidParent = this.findNextXidNode(node);
                        if (!xidParent.eachcount) {{
                            xidParent.eachcount = 0;
                        }}
                        xidParent.eachcount++;
                        var eachId = xidParent.getAttribute('xid') + '-each-' + xidParent.eachcount
                        node.setAttribute('eachid', eachId);
                        if (Array.isArray(array)) {{
                            if (!window[eachId]) {{
                                window[eachId] = new Watcher(array, node, this, segment.content, (action, data) => {{  }})
                            }}
                            else {{
                                node.parentNode.replaceChild(window[eachId].parentNode, node)
                            }}
                        }}
                    }} else if (segment.directive === '#switch') {{
                        segment.cases = this.parseSwitchCases(segment.content);
                        segment.default = this.parseSwitchDefault(segment.content);
                        var switchValue = this.evaluateExpression(segment.expression, data);
                        var switchCase = segment.cases.find(c => c.value === switchValue);
                        if (switchCase) {{
                            this.renderTemplate(this.parseTemplate(switchCase.content[0].value), data, node);
                        }}
                        else if(segment.default){{
                            this.renderTemplate(this.parseTemplate(segment.default.content[0].value), data, node);
                        }}
                    }}
                }} else if (segment.type === 'variable') {{
                    var tmp = document.createElement('div')
                    tmp.innerHTML = this.evaluateExpression(segment.expression, data);
                    node.appendChild(tmp);
                }}
            }}
        }}
    }}
    findNextXidNode(node) {{
        // Start climbing up the node tree until reaching the document root
        let nextXid;
        while (node && node !== document) {{
            // Check if the current node has the 'xid' attribute
            if (node.getAttribute('xid')) {{
                return node; // Found the node with 'xid' attribute, return it
            }}
            // Move to the parent node
            node = node.parentNode;
        }}
        // If no node with 'xid' attribute found, return null
        return null;
    }}
    parseSwitchCases(content) {{
        const regex =  /-\[\s*#case\s*(.*?)\](.*?)(?=-\[\s*#(case|default)\s*(.*?)\]|-\[\s*\/\s*\])/gs;
        const cases = [];
        let match;
        content = content + '-[/]'

        while ((match = regex.exec(content)) !== null) {{
            const [fullMatch, value, caseContent] = match;
            cases.push({{
                value: value.trim().replace(/""/g, '').replace(/`/g, '').replace(/'/g, ''),
                content: this.parseTemplate(caseContent.trim())
            }});
        }}

        return cases;
    }}
    parseSwitchDefault(content) {{
        const regex =  /-\[\s*#default\s*(.*?)\](.*?)(?=-\[\s*#case\s*(.*?)\]|-\[\s*\/\s*\])/gs;
        const cases = [];
        let match;
        content = content + '-[/]'

        while ((match = regex.exec(content)) !== null) {{
            const [fullMatch, value, caseContent] = match;
            cases.push({{
                value: value.trim().replace(/""/g, '').replace(/`/g, '').replace(/'/g, ''),
                content: this.parseTemplate(caseContent.trim())
            }});
        }}

        return cases[0] ?? null;
    }}
    evaluateExpression(expression, data) {{
        if (expression.startsWith('#')) {{
            const [directive, ...rest] = expression.split(/\s+/); // Get the directive and the rest of the expression
            const condition = rest.join(' ').trim(); // Extract the condition part

            switch (directive) {{
                case '#if':
                    return this.evaluateCondition(condition, data);
                case '#each':
                    return this.evaluateEach(condition, data);
                case '#switch':
                    return this.evaluateSwitch(condition, data);
                // Add more cases for other directives as needed
                default:
                    return '';
            }}
        }}

        // Otherwise, it's a variable expression
        const properties = expression.split('.');
        let value = data;
        if(expression.includes('""') ||expression.includes(""'"") ||expression.includes('`')){{
            return expression;
        }}
        for (const prop of properties) {{
            value = value[prop];
            if (value === undefined) return '';
        }}

        return value;
    }}
    evaluateCondition(condition, data) {{
        try {{
        return new Function('data', `
    const safeData = data;
    const {{ ${{Object.keys(data).map(key => `${{key}} = safeData.${{key}} ?? null`).join(', ')}} }} = safeData;
    return ${{condition}};
`)(data);
        }} catch (error) {{
            console.error('Error evaluating condition:', error);
            return false;
        }}
    }}

    evaluateEach(expression, data) {{
        const items = this.evaluateExpression(expression, data);
        return Array.isArray(items) ? items : [];
    }}
    evaluateSwitch(expression, data) {{
        const switchValue = this.evaluateExpression(expression, data);
        const cases = [];
        return {{ switchValue, cases }};
    }}
    compareObjectLists(oldList,newList) {{

        const itemsWithNewValues = [];

        for (const newItem of newList) {{
            const matchingOldItem = oldList.find(oldItem => oldItem.name === newItem.name);
            var isArray = false
            if (matchingOldItem) {{
                if (Array.isArray(matchingOldItem.newValue)) {{
                    isArray = true
                    if (matchingOldItem.newValue.length !== newItem.newValue.length) {{
                        itemsWithNewValues.push(newItem);
                    }}
                    for (var i = 0, len = matchingOldItem.newValue.length; i < len; i++) {{
                        if (matchingOldItem.newValue[i] !== newItem.newValue[i]) {{

                            itemsWithNewValues.push(newItem);
                            break;
                        }}
                    }}
                }}
                if (matchingOldItem.newValue !== newItem.newValue && !isArray) {{
                    itemsWithNewValues.push(newItem);
                }}
            }}
        }}
        return itemsWithNewValues;
    }}
    compareVarsToList() {{
        var oldList = this.Variables;
        var newList = this.getVariables();
        const itemsWithNewValues = [];

        for (const newItem of newList) {{
            const matchingOldItem = oldList.find(oldItem => oldItem.name === newItem.name);
            var isArray = false
            if (matchingOldItem) {{
                if (Array.isArray(matchingOldItem.newValue)) {{
                    isArray = true
                    if (matchingOldItem.newValue.length !== newItem.newValue.length) {{
                    itemsWithNewValues.push(newItem);
                    }}
                    for (var i = 0, len = matchingOldItem.newValue.length; i < len; i++) {{
                        if (matchingOldItem.newValue[i] !== newItem.newValue[i]) {{

                            itemsWithNewValues.push(newItem);
                            break;
                        }}
                    }}
                }}
                if (matchingOldItem.newValue !== newItem.newValue && !isArray) {{
                    itemsWithNewValues.push(newItem);
                }}
            }}
        }}
        return itemsWithNewValues;
    }}
    replaceVariables() {{
        // Iterate over each binding
        for (var key in this.VariableInfo) {{
            this.VariableInfo[key].forEach((item) => {{
                var originalContent = item.node.originalContent;

                // Replace all occurrences of the variable within the original content
                this.RunTemplater(originalContent, item.node);

                // Update the node's HTML content
            }});

        }}
    }}
    updateVariables() {{
        // Iterate over each binding
        for (var key in this.VariableInfo) {{
            this.VariableInfo[key].forEach((item) => {{
                var originalContent = item.node.originalContent;

                // Replace all occurrences of the variable within the original content
                if(!originalContent.includes(""-[#each"")){{
                  this.RunTemplater(originalContent, item.node);
                }}

                // Update the node's HTML content
            }});

        }}
    }}
    createBindings() {{
        // Get all nodes in the DOM
        var allNodes = this.Element.querySelectorAll(""*"");
        if (!this.VariableInfo[""found""]) {{
            this.VariableInfo[""found""] = [];
        }}
        else {{
            this.VariableInfo[""found""] = [];
        }}

        // Iterate over each node
        allNodes.forEach((node) => {{
            var text = node.innerHTML.replace(/<[^>]+>[\s\S]*[^<]*<\/[^>]+>/g, '')
            // Check if the node's text content contains a binding
            if (text.includes(""-["")) {{
                // Store the original content with placeholders
                node.originalContent = node.innerHTML;
                this.VariableInfo[""found""].push({{ node }})
            }}
            
        }});
    }}
    getVariables() {{
        var obj = this;
        var variables = [];
        for (var key in obj) {{
            var found = false;
            for (var func in this.getFunctions()){{
                if(func === key){{
                    found = true;
                }}
            }}
            switch(key){{
                case ""Variables"":
                    found = true;
                    break;
                case ""VariableInfo"":
                    found = true;
                    break;
                case ""Element"":
                    found = true;
                    break;
                case ""VirtualNode"":
                    found = true;
                    break;
                 case ""MutationStack"":
                    found = true;
                    break;
                 case ""ScriptRender"":
                    found = true;
                    break;
                 case ""ShouldRender"":
                    found = true;
                    break;
                  case ""LoadingVirtualDOM"":
                    found = true;
                    break;
                  case ""Shadow"":
                    found = true;
                    break;

            }}
            if(found){{
                continue;
            }}
            if (obj.hasOwnProperty(key)) {{
                if (Array.isArray(obj[key])) {{
                    variables.push({{ name: key, newValue: Array.from(obj[key]) }});
                    continue;
                }}
                variables.push({{ name: key, newValue: obj[key] }});
            }}
        }}
        return variables;
    }}


    getFunctions(){{
        const methods = Object.getOwnPropertyNames(this)
              .filter(prop => typeof this[prop] === 'function' && prop !== 'constructor');
        return methods;
    }}
    async Render(){{
        try{{
            if(this.ShouldRender === true){{
              await this.replaceElements(this.Name.toLowerCase());
              this.ShouldRender = false;
            }}
        }}
        catch(ex){{console.log(ex);
        }}
    }}
}}");
                await this.SearchForXavierNodesAndChildren(root, true, assembly);
                foreach (var xav in XavierNodes)
                {
                    var xavier = xav as XavierNode;
                    sb.Append((xav as XavierNode).ClassBody(this));
                    wb.Append($@"'{xavier.Name}',");
                }


                this.JSModule = $@"(async function(){{
{sb.ToString()}
    var renderer = setInterval(function () {{
        var nameList = [{wb.ToString()}];
        var count = 0;
        nameList.forEach((name) => {{
            document.querySelectorAll(name).forEach((element) => {{
                count++
                if (!element.hasAttribute('xid')) {{
                    var xnode = eval('new ' + name.toUpperCase() + '()');
                    xnode.Element = element;
                    xnode.index = count;
                    xnode.Render();
                }}
            }})
        }})
    }}, 10);
{EFToJS.GenerateWhereMethodJS()}
{EFToJS.GenerateWFirstMethodJS()}
{EFToJS.GenerateSingleOrDefaultMethodJS()}
}})()";

                var file = $"{XavierName}.js";
            wb.Clear();
                if (File.Exists(file))
                {
                    if (XavierNodes != null)
                    {
                foreach (var xav in XavierNodes)
                {
                var xavier = xav as XavierNode;
                    wb.Append($"var {xavier.Name} = new {xavier.Name.ToUpper()}();" +
                        $"if(window.location.pathname === {xavier.Name}.Route && {xavier.Name}.ShouldRender === true || {xavier.Name}.Route === '' && {xavier.Name}.ShouldRender === true){{" +
                $"await {xavier.Name}.renderXidElements(document.body);"+
                $"}}");
                }
                        StringBuilder check = new StringBuilder();
                        XavierNodes.ForEach(n => { check.Append((n as XavierNode).ClassBody(this)); });
                        if (File.ReadAllText(file).Length == ($"(async function(){{ { check.ToString()} { wb.ToString()} {EFToJS.GenerateWhereMethodJS()} {EFToJS.GenerateWFirstMethodJS()} {EFToJS.GenerateSingleOrDefaultMethodJS()} }})()").Length)
                        {

                        }
                        else
                        {
                            Console.WriteLine("Streaming changes to " + file);
                            WriteModule();
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Writing Xavier Module named " + file);
                    WriteModule();
                }
                GC.Collect();
            });
        }
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public async Task Init(string root, string destination, List<DbContext> contexts, Assembly assembly)
        {
            StaticRoot = root;
            PyEngine = Python.CreateEngine();
            XavierName = destination;
            await Task.Run(async () =>
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder wb = new StringBuilder();

                await this.SearchForXavierNodesAndChildren(root, true, assembly);
                foreach (var xav in XavierNodes)
                {

                var xavier = xav as XavierNode;
                    sb.Append(xavier.ClassBody(this));
                    wb.Append($"var {xavier.Name} = new {xavier.Name.ToUpper()}();" +
                        $"if(window.location.pathname === {xavier.Name}.Route && {xavier.Name}.ShouldRender === true || {xavier.Name}.Route === '' && {xavier.Name}.ShouldRender === true){{" +
                $"await {xavier.Name}.renderXidElements(document.body);"+
                $"}}");

                }
                this.Contexts = contexts;
                this.JSModule = $"(async function(){{ {sb.ToString()} {wb.ToString()} {EFToJS.GenerateWhereMethodJS()} {EFToJS.GenerateWFirstMethodJS()} {EFToJS.GenerateSingleOrDefaultMethodJS()} }})()";

                try
                {
                    foreach (var c in Contexts)
                    {
                        var efFile = $"{XavierName}.{c.GetType().Name}.js";
                        if (File.Exists(efFile))
                        {
                            if (File.ReadAllText(efFile).Length == TestJavascriptFile(efFile, c).Length)
                            {

                            }
                            else
                            {
                                Console.WriteLine("Streaming changes to " + efFile);
                                WriteJavascriptFile(efFile, c);
                            }
                        }
                        else
                        {

                            Console.WriteLine("Writing EF Core module to " + efFile);
                            WriteJavascriptFile(efFile, c);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                var file = $"{XavierName}.js";
            wb.Clear();

                if (File.Exists(file))
                {
                    if (XavierNodes != null)
                    {
                foreach (var xav in XavierNodes)
                {
                var xavier = xav as XavierNode;
                    wb.Append($"var {xavier.Name} = new {xavier.Name.ToUpper()}();" +
                        $"if(window.location.pathname === {xavier.Name}.Route && {xavier.Name}.ShouldRender === true || {xavier.Name}.Route === '' && {xavier.Name}.ShouldRender === true){{" +
                $"await {xavier.Name}.renderXidElements(document.body);"+
                $"}}");

                }
                        StringBuilder check = new StringBuilder();
                        XavierNodes.ForEach(n => { check.Append((n as XavierNode).ClassBody(this)); });
                        if (File.ReadAllText(file).Length == ($"(async function(){{ { check.ToString()} { wb.ToString()} {EFToJS.GenerateWhereMethodJS()} {EFToJS.GenerateWFirstMethodJS()} {EFToJS.GenerateSingleOrDefaultMethodJS()} }})()").Length)
                        {

                        }
                        else
                        {
                            Console.WriteLine("Streaming changes to " + file);
                            WriteModule();
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Writing Xavier Module named " + file);
                    WriteModule();
                }
            });
        }

        public void WriteModule()
        {
            var file = $"{XavierName}.js";
            File.WriteAllText(file, JSModule);
        }

        public void WriteEF(DbContext context)
        {
            var file = Environment.CurrentDirectory + $"/{XavierName}.{context.GetType().Name}.js";
            var item = EFToJS.TranslateEFToJS(context);

            File.WriteAllText(file, item);
            WriteJavascriptFile(file, context);
        }
        public static string TestJavascriptFile(string fileName, DbContext dbContext)
        {
            StringBuilder sw = new StringBuilder();

            sw.AppendLine("//THIS IS A GENERATED FILE - Do not alter");

            //// create a connection string to connect to the database
            //string connectionString = dbContext.Database.GetDbConnection().ConnectionString;

            //// create a new instance of the sequelize module
            //sw.AppendLine($"const connect = \"{connectionString}\";");

            // loop through the available models
            foreach (var modelType in dbContext.Model.GetEntityTypes())
            {
                try
                {
                    var startIndex = (modelType.Name.LastIndexOf(".") >= 0) ? modelType.Name.LastIndexOf(".") + 1 : 0;
                    // get the model name from the model type
                    string modelName = modelType.Name.Substring(startIndex, modelType.Name.Length - startIndex).Replace("<string>", "");

                    sw.AppendLine($@"export class {modelName.Replace("+", "")} {{
  constructor(){{");

                    // loop through the model properties
                    foreach (var property in modelType.GetProperties())
                    {
                        // get the property name and type
                        string propertyName = property.Name;
                        string propertyType = property.ClrType.Name;
                        sw.AppendLine($"     this.{propertyName}= {{}}");
                    }
                    sw.AppendLine("} }");
                    sw.AppendLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            // loop through the db sets
            foreach (var dbSet in dbContext.GetType().GetProperties())
            {
                // get the db set name
                string dbSetName = dbSet.Name;

                sw.AppendLine($"export const {dbSetName} = [];");
            }
            sw.AppendLine(EFToJS.TranslateEFToJS(dbContext));
            return sw.ToString();
        }
        public async void WriteJavascriptFile(string fileName, DbContext dbContext)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine("//THIS IS A GENERATED FILE - Do not alter");

                // create a connection string to connect to the database
                // string connectionString = dbContext.Database.GetDbConnection().ConnectionString;

                // create a new instance of the sequelize module
                // sw.WriteLine($"const connect = \"{connectionString.Replace("\\", "/")}\";");

                // loop through the available models
                foreach (var modelType in dbContext.Model.GetEntityTypes())
                {
                    try
                    {
                        var startIndex = (modelType.Name.LastIndexOf(".") >= 0) ? modelType.Name.LastIndexOf(".") + 1 : 0;
                        // get the model name from the model type
                        string modelName = modelType.Name.Substring(startIndex, modelType.Name.Length - startIndex).Replace("<string>", "");

                        sw.WriteLine($@"export class {modelName.Replace("+", "")} {{
  constructor(){{");

                        // loop through the model properties
                        foreach (var property in modelType.GetProperties())
                        {
                            // get the property name and type
                            string propertyName = property.Name;
                            string propertyType = property.ClrType.Name;

                            sw.WriteLine($"     this.{propertyName}= {{}}");
                        }

                        sw.WriteLine("} }");
                        sw.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                // loop through the db sets
                foreach (var dbSet in dbContext.GetType().GetProperties())
                {
                    // get the db set name
                    string dbSetName = dbSet.Name;

                    sw.WriteLine($"export const {dbSetName} = [];");
                }
                sw.WriteLine(EFToJS.TranslateEFToJS(dbContext));
                await dbContext.DisposeAsync();
                sw.Close();
            }

        }
        public async Task SearchForXavierNodesAndChildren(string searchDir, bool searchSubdirectories, Assembly assembly)
        {
                List<object> XavierNodesAndChildren = new List<object>();
            await Task.Run(() =>
            {
                try
                {
                    // Search the directory for all .xavier files
                    string[] xavierNodes = Directory.GetFiles(searchDir, "*.xavier",

                          (searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                    // Check if any .xavier files were found
                    if (xavierNodes.Length > 0)
                    {
                        // If xavier files were found, check if they have any children that inherit from the
                        // XavierNode class
                        for (int i = 0; i < xavierNodes.Length; i++)
                        {
                            string XavierNodePath = xavierNodes[i];
                            var XavierNode = new XavierNode(XavierNodePath, assembly, this);
                            var args = new object[1];
                            var node = new object();
                            args[0] = XavierNode;
                            try
                            {
                                node = Activator.CreateInstance(assembly.GetType(assembly.FullName.Split(",")[0] + "." + XavierNode.Name), args);
                                //Console.WriteLine(XavierNode.Content());
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (!(XavierNodes.Where(n => n.GetType() == node.GetType()).Count() > 0))
                            {
                                XavierNodes.Add(node);
                            }
                            // Check if the .xavier file has any .xavier.cs children
                        }
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message + "Here is the problem in Search for xavier nodes");
                }
            });
        }
        public void Dispose()
        {
            XavierNodes.Clear();
            JSModule = null;
        }
    }
}
