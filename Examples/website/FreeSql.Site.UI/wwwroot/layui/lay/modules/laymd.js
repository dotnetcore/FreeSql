layui.define(['jquery'], function(exports) {
    "use strict";

    var $ = layui.$,
        MOD_NAME = 'laymd',
        JS_PATH;

    //获取JS所在路径
    if (document.currentScript) {
        JS_PATH = document.currentScript.src;
    } else {
        var js = document.scripts, last = js.length - 1, src;
        for(var i = last; i > 0; i--){
            if(js[i].readyState === 'interactive'){
                src = js[i].src;
                break;
            }
        }
        JS_PATH = src || js[last].src;
    }
    JS_PATH = JS_PATH.substring(0, JS_PATH.lastIndexOf('/') + 1);

    //加载CSS
    layui.link(JS_PATH + 'laymd.css');

    //实例化
    var MD = function (id, options) {

        //默认配置项
        var config = {
            tools: [
                'bold', 'italic', 'underline', 'del',
                '|',
                'left', 'center', 'right',
                '|',
                'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
                '|',
                'hr', 'link', 'code', 'ol', 'ul', 'tl',
                '|',
                'table', 'quote', 'toc', 'img',
                '|',
                'full', 'preview'
            ],
            height: 280
        };

        //合并配置项
        config = $.extend({}, config, options);

        //相关元素
        var EL = {}, THIS = this;

        //获取编辑器容器
        EL.$div = $(typeof(id) === 'string' ? '#' + id : id).addClass('layui-laymd');

        //获取默认值
        var initValue = EL.$div.text();

        //设置要显示的工具
        var toolBar = [];
        layui.each(config.tools, function(index, item){
            tools[item] &&  toolBar.push(tools[item]);
        });

        //载入元素
        EL.$div.html([
            '<div class="layui-unselect layui-laymd-tool">' + toolBar.join('') + '</div>',
                '<div class="layui-laymd-area">',
                    '<textarea spellcheck="false"></textarea>',
                    '<iframe></iframe>',
                '</div>',
            '</div>'
        ].join(''));

        //设置编辑框和预览框
        EL.$body = $('body');
        EL.$div.find('.layui-laymd-area').height(config.height);
        EL.$div.find('textarea').attr('id', EL.$div.attr('name') || EL.$div.prop('id'))
        EL.$textArea = EL.$div.find('textarea').attr('name', EL.$div.attr('name') || EL.$div.prop('id')).val(initValue);
        EL.$iframe = EL.$div.find('iframe');

        //设置预览默认样式
        EL.$iframe.contents().find('head').append('<link rel="stylesheet" href="' + JS_PATH + 'preview.css' + '">');

        //获取DOM
        var textArea = EL.$textArea[0];

        //==============================================================================================================

        //绑定按键事件
        EL.$textArea.on('keydown', function (e) {
            if (e.ctrlKey) {
                if (e.shiftKey && e.keyCode === 90) { //ctrl + shift + z
                    actions.redo.call(THIS, e, this, EL);
                } else if (keyMap[e.keyCode]) {
                    e.preventDefault();
                    actions[keyMap[e.keyCode]].call(THIS, e, this, EL);
                }
            } else {
                if (e.keyCode === 9) { //tab
                    e.preventDefault();
                    actions.tab.call(THIS, e, this, EL);
                }
            }
        });

        //绑定按钮事件
        EL.$textArea.on('keyup', function (e) {
            if (e.keyCode === 13) {
                e.preventDefault();
                actions.enter.call(THIS, e, this, EL);
            }
        });

        //滚动事件
        EL.$textArea.scroll(function () {
            var ifrBody = EL.$iframe.contents().find('body')[0];
            if (ifrBody.scrollHeight > EL.$iframe.height()) {
                var p = (ifrBody.scrollHeight - EL.$iframe.height()) / (textArea.scrollHeight - EL.$textArea.outerHeight());
                EL.$iframe[0].contentWindow.scroll(0, this.scrollTop * p);
            }
        });

        //输入法输入事件
        var composition = false, preText, sufText;
        EL.$textArea.on('input', function (e) {
            composition || actions.input.call(THIS, e, this, EL);
        }).on('compositionstart', function (e) {
            preText = this.value;
            composition = true;
        }).on('compositionend', function (e) {
            composition = false;
            sufText = this.value;
            preText === sufText || actions.input.call(THIS, e, this, EL);
        });

        //工具栏事件
        EL.$div.find('.layui-laymd-tool > i').on('click', function (e) {
            actions[$(this).attr('laymd-event')].call(THIS, e, this, EL);
        });

        //==============================================================================================================

        //事件绑定
        this.on = function (event, callback) {
            layui.onevent.call(this, MOD_NAME, MOD_NAME + '(' + event + ')', callback);
        };

        //执行某个动作
        this.do = function (action, event, element, params) {
            actions[action] && actions[action].call(THIS, event, element, EL, params);
        };

        //定时存储操作记录
        setInterval(function () {
            THIS.history.undo(true);
        }, 1500);

        //操作记录
        this.history = {
            _undo: [textArea.value],
            _redo: [],
            undo: function (record) {
                if (record) {
                    var text = textArea.value;
                    if (this._undo[this._undo.length - 1] === text) {
                        return false;
                    } else {
                        this._undo.push(text);
                        this._undo.length > 500 && this._undo.shift();
                    }
                } else {
                    this.undo(true);
                    if (this._undo.length > 1) {
                        this._redo.push(this._undo.pop());
                        textArea.value = this._undo[this._undo.length - 1];
                        actions.change.call(THIS, null, null, EL);
                    }
                }
            },
            redo: function (flush) {
                if (flush && this._redo.length) {
                    this._redo = [];
                } else {
                    if (this._redo.length > 0) {
                        textArea.value = this._redo.pop();
                        this.undo(true);
                        actions.change.call(THIS, null, null, EL);
                    }
                }
            }
        };

        //==============================================================================================================

        /**
         * 获取选中位置
         * @returns {{start: number, end: number, text: string}}
         */
        this.getRangeData = function () {
            textArea.focus();
            return {
                start: textArea.selectionStart,
                end: textArea.selectionEnd,
                text: textArea.value.substring(textArea.selectionStart, textArea.selectionEnd)
            };
        };

        /**
         * 替换选中数据
         * @param rangeData {{start: *|number, end: *|number, text: *|string}}
         */
        this.setRangeData = function (rangeData) {
            textArea.focus();
            if (typeof rangeData.text === 'string') {
                var value = textArea.value;
                if (textArea.setRangeText) {
                    textArea.setRangeText(rangeData.text);
                } else {
                    var range = this.getRangeData(),
                        pre = value.substring(0, range.start),
                        suf = value.substring(range.end);
                    textArea.value = pre + rangeData.text + suf;
                    textArea.selectionStart = range.start;
                    textArea.selectionEnd = range.start + rangeData.text.length;
                }
                value === textArea.value || EL.$textArea.trigger('input');
            }
            if (typeof rangeData.start === 'number') {
                textArea.selectionStart = rangeData.start;
            }
            if (typeof rangeData.end === 'number') {
                textArea.selectionEnd = rangeData.end;
            }
        };

        /**
         * 获取选中文本
         * @returns {string}
         */
        this.getRangeText = function () {
            return this.getRangeData().text;
        };

        /**
         * 设置选中文本
         * @param text
         */
        this.setRangeText = function (text) {
            this.setRangeData({text: text});
        };

        /**
         * 获取光标所在行的数据
         * @param line
         * @returns {{start: number, end: number, line: number, text: string}}
         */
        this.getLineData = function (line) {
            textArea.focus();
            var lineData = {},
                text = textArea.value,
                lines = text.split("\n");

            lineData.start = 0;
            lineData.line = typeof line === 'number' ? line : text.substring(0, textArea.selectionEnd).split("\n").length - 1;
            lineData.text = lines[lineData.line] || '';
            for (var i = 0; i < lineData.line; i++) {
                lineData.start += lines[i].length + 1;
            }
            lineData.end = lineData.start + lineData.text.length;
            return lineData;
        };

        /**
         * 设置光标所在行的数据
         * @param lineData {{start: *|number, end: *|number, line: *|number, text: *|string}}
         */
        this.setLineData = function (lineData) {
            textArea.focus();
            if (typeof lineData.text === 'string') {
                var line = this.getLineData(lineData.line),
                    value = textArea.value,
                    pre = value.substring(0, line.start),
                    suf = value.substring(line.end);
                textArea.value = pre + lineData.text + suf;
                textArea.selectionStart = textArea.selectionEnd = line.start + lineData.text.length;
                value === textArea.value || EL.$textArea.trigger('input');
            }
            if (typeof lineData.start === 'number') {
                textArea.selectionStart = lineData.start;
            }
            if (typeof lineData.end === 'number') {
                textArea.selectionEnd = lineData.end;
            }
        };

        /**
         * 获取光标所在行的文本
         * @returns {string}
         */
        this.getLineText = function (line) {
            return this.getLineData({line: line}).text;
        };

        /**
         * 设置光标所在行的文本
         * @param text
         * @param line
         */
        this.setLineText = function (text, line) {
            this.setLineData({text: text, line: line});
        };

        //==============================================================================================================

        //获取编辑器的文本
        this.getText = function () {
            return textArea.value;
        };

        //设置预览HTML
        this.setPreview = function (html) {
            EL.$iframe.contents().find('body').html(html);
        };

        //设置超链接
        this.setLink = function (link, text, title) {
            actions.link.call(THIS, null, null, EL, {
                link: link,
                text: text,
                title: title
            });
        };

        //设置图片
        this.setImg = function (src, alt, title) {
            actions.img.call(THIS, null, null, EL, {
                src: src,
                alt: alt,
                title: title
            });
        };
    };

    //所有工具
    var tools = {
        bold: '<i class="laymd-tool-b" title="加粗" laymd-event="bold">B</i>',
        italic: '<i class="laymd-tool-i" title="斜体" laymd-event="italic">I</i>',
        underline: '<i class="laymd-tool-u" title="下划线" laymd-event="underline">U</i>',
        del: '<i class="laymd-tool-d" title="删除线" laymd-event="del">D</i>',
        '|': '<span></span>',
        h1: '<i class="laymd-tool-h1" title="h1" laymd-event="h1">h1</i>',
        h2: '<i class="laymd-tool-h2" title="h2" laymd-event="h2">h2</i>',
        h3: '<i class="laymd-tool-h3" title="h3" laymd-event="h3">h3</i>',
        h4: '<i class="laymd-tool-h4" title="h4" laymd-event="h4">h4</i>',
        h5: '<i class="laymd-tool-h5" title="h5" laymd-event="h5">h5</i>',
        h6: '<i class="laymd-tool-h6" title="h6" laymd-event="h6">h6</i>',
        hr: '<i class="laymd-tool-hr" title="换行符" laymd-event="hr">—</i>',
        link: '<i class="laymd-tool-link" title="超链接" laymd-event="link">A</i>',
        code: '<i class="laymd-tool-code" title="代码" laymd-event="code">/</i>',
        ol: '<i class="laymd-tool-ol" title="有序列表" laymd-event="ol">ol</i>',
        ul: '<i class="laymd-tool-ul" title="无序列表" laymd-event="ul">ul</i>',
        tl: '<i class="laymd-tool-tl" title="任务列表" laymd-event="tl">tl</i>',
        table: '<i class="laymd-tool-table" title="表格" laymd-event="table">T</i>',
        quote: '<i class="laymd-tool-quote" title="引用" laymd-event="quote">Q</i>',
        toc: '<i class="laymd-tool-toc" title="导航" laymd-event="toc">TOC</i>',
        left: '<i class="laymd-tool-left" title="居左" laymd-event="left">L</i>',
        center: '<i class="laymd-tool-center" title="居中" laymd-event="center">C</i>',
        right: '<i class="laymd-tool-right" title="居右" laymd-event="right">R</i>',
        img: '<i class="laymd-tool-img" title="图片" laymd-event="img">IMG</i>',
        full: '<i class="laymd-tool-full" title="全屏" laymd-event="full">↗</i>',
        preview: '<i class="laymd-tool-preview" title="预览" laymd-event="preview">√</i>'
    };

    //热键数组
    var keyMap = [];
    keyMap[66] = 'bold'; //ctrl + b
    keyMap[73] = 'italic'; //ctrl + i
    keyMap[85] = 'underline'; //ctrl + u
    keyMap[68] = 'del'; //ctrl + d
    keyMap[37] = 'left'; //ctrl + ←
    keyMap[38] = 'center'; //ctrl + ↑
    keyMap[39] = 'right'; //ctrl + →
    keyMap[49] = 'h1'; //ctrl + 1
    keyMap[50] = 'h2'; //ctrl + 2
    keyMap[51] = 'h3'; //ctrl + 3
    keyMap[52] = 'h4'; //ctrl + 4
    keyMap[53] = 'h5'; //ctrl + 5
    keyMap[54] = 'h6'; //ctrl + 6
    keyMap[189] = 'hr'; //ctrl + -
    keyMap[76] = 'link'; //ctrl + l
    keyMap[191] = 'code'; //ctrl + /
    keyMap[81] = 'quote'; //ctrl + q
    keyMap[89] = 'redo'; //ctrl + y
    keyMap[90] = 'undo'; //ctrl + z

    //事件列表
    var actions = {
        bold: function (event, element, EL, params) {
            var range = this.getRangeData();
            this.setRangeData({
                text: '**' + range.text + '**',
                start: range.end + 2,
                end: range.end + 2
            });
        },
        italic: function (event, element, EL, params) {
            var range = this.getRangeData();
            this.setRangeData({
                text: '*' + range.text + '*',
                start: range.end + 1,
                end: range.end + 1
            });
        },
        underline: function (event, element, EL, params) {
            var range = this.getRangeData();
            this.setRangeData({
                text: '++' + range.text + '++',
                start: range.end + 2,
                end: range.end + 2
            });
        },
        del: function (event, element, EL, params) {
            var range = this.getRangeData();
            this.setRangeData({
                text: '~~' + range.text + '~~',
                start: range.end + 2,
                end: range.end + 2
            });
        },
        left: function (event, element, EL, params) {
            this.setLineText(this.getLineText().replace(/^ *(:-:|--:) /, ''));
        },
        center: function (event, element, EL, params) {
            this.setLineText(':-: ' + this.getLineText().replace(/ *(^:-:|--:) /, ''));
        },
        right: function (event, element, EL, params) {
            this.setLineText('--: ' + this.getLineText().replace(/^ *(:-:|--:) /, ''));
        },
        h1: function (event, element, EL, params) {
            this.setLineText('# ' + this.getLineText().replace(/^ *#+ /, ''));
        },
        h2: function (event, element, EL, params) {
            this.setLineText('## ' + this.getLineText().replace(/^ *#+ /, ''));
        },
        h3: function (event, element, EL, params) {
            this.setLineText('### ' + this.getLineText().replace(/^ *#+ /, ''));
        },
        h4: function (event, element, EL, params) {
            this.setLineText('#### ' + this.getLineText().replace(/^ *#+ /, ''));
        },
        h5: function (event, element, EL, params) {
            this.setLineText('##### ' + this.getLineText().replace(/^ *#+ /, ''));
        },
        h6: function (event, element, EL, params) {
            this.setLineText('###### ' + this.getLineText().replace(/^ *#+ /, ''));
        },
        hr: function (event, element, EL, params) {
            var range = this.getRangeData();
            this.setRangeData({
                text: "\n---\n",
                start: range.start + 5,
                end: range.start + 5
            });
        },
        link: function (event, element, EL, params) {
            var range = this.getRangeData();
            if (params) {
                var text = params.text || range.text || params.link,
                    title = params.title || text || '',
                    link = '[' + text + '](' + params.link + (title ? (' "' + title + '"') : '') + ')';
                this.setRangeData({
                    text: link,
                    start: range.start + link.length,
                    end: range.start + link.length
                });
            } else {
                var textLen = range.text.length,
                    text = textLen ? range.text : 'text',
                    title = textLen ? range.text : 'title';
                this.setRangeData({
                    text: '[' + text + '](http://link-address "' + title + '")',
                    start: textLen ? (range.start + textLen + 3) : (range.start + 1),
                    end: textLen ? (range.start + textLen + 22) : (range.start + 5),
                });
            }
        },
        code: function (event, element, EL, params) {
            var range = this.getRangeData(),
                line = this.getLineData();
            if (range.text || line.text) {
                this.setRangeData({
                    text: '`' + range.text + '`',
                    start: range.end + 1,
                    end: range.end + 1
                });
            } else {
                this.setLineData({
                    text: '```\n\n```',
                    start: line.start + 4,
                    end: line.start + 4
                });
            }
        },
        ol: function (event, element, EL, params) {
            this.setLineText(this.getLineText().replace(/^( *)(?:(?:(?:\d+\.)|(?:-(?: \[[ x]])?)) )?(.*)/, '$11. $2'));
        },
        ul: function (event, element, EL, params) {
            this.setLineText(this.getLineText().replace(/^( *)(?:(?:(?:\d+\.)|(?:-(?: \[[ x]])?)) )?(.*)/, '$1- $2'));
        },
        tl: function (event, element, EL, params) {
            this.setLineText(this.getLineText().replace(/^( *)(?:(?:(?:\d+\.)|(?:-(?: \[[ x]])?)) )?(.*)/, '$1- [ ] $2'));
        },
        enter: function (event, element, EL, params) {
            var line = this.getLineData(),
                preLine = this.getLineData(line.line - 1);
            var match = /^( *)((?:(?:\d+\.)|(?:-(?: \[[ x]])?)) )?(.*)/.exec(preLine.text);
            if (match[2]) {
                if (match[3] === '') {
                    this.setLineText('', preLine.line);
                    this.setLineText('', line.line);
                } else if (match[2].length === 3) {
                    var m = /^(\d+)\. /.exec(match[2]);
                    this.setLineText(match[1] + (parseInt(m[1]) + 1) + '. ' + line.text);
                } else {
                    this.setLineText(match[1] + match[2] + line.text);
                }
            }
        },
        table: function (event, element, EL, params) {
            var range = this.getRangeData(),
                table = [
                    this.getLineText() ? '\n' : '',
                    'header 1 | header 2',
                    '--- | ---',
                    'row 1 col 1 | row 1 col 2',
                    'row 2 col 1 | row 2 col 2',
                    '\n'
                ].join("\n");
            this.setRangeData({
                text: table,
                start: range.start + table.length,
                end: range.start + table.length
            });
        },
        quote: function (event, element, EL, params) {
            this.setLineText('> ' + this.getLineText().replace(/^ *> /, ''));
        },
        toc: function (event, element, EL, params) {
            var range = this.getRangeData();
            this.setRangeData({
                text: '\n[TOC]\n',
                start: range.start + 7,
                end: range.start + 7
            });
        },
        img: function (event, element, EL, params) {
            var range = this.getRangeData();
            if (params) {
                var alt = params.alt || range.text || params.title || params.src,
                    title = params.title || alt || '',
                    src = '[' + alt + '](' + params.src + (title ? (' "' + title + '"') : '') + ')';
                this.setRangeData({
                    text: src,
                    start: range.start + src.length,
                    end: range.start + src.length
                });
            } else {
                var textLen = range.text.length,
                    alt = textLen ? range.text : 'alt',
                    title = textLen ? range.text : 'title';
                this.setRangeData({
                    text: '![' + alt + '](http://link-address "' + title + '")',
                    start: textLen ? (range.start + textLen + 4) : (range.start + 2),
                    end: textLen ? (range.start + textLen + 23) : (range.start + 5),
                });
            }
        },
        tab: function (event, element, EL, params) {
            var range = this.getRangeData();
            this.setRangeData({text: '    ', start: range.start + 4, end: range.start + 4});
        },
        input: function (event, element, EL, params) {
            this.history.redo(true);
            actions.change.call(this, event, element, EL, params);
        },
        redo: function (event, element, EL, params) {
            this.history.redo(params);
        },
        undo: function (event, element, EL, params) {
            this.history.undo(params);
        },
        change: function (event, element, EL, params) {
            layui.event.call(this, MOD_NAME, MOD_NAME + '(change)');
        },
        full: function (event, element, EL, params) {
            if (EL.$div.hasClass('layui-laymd-full')) {
                EL.$div.removeClass('layui-laymd-full');
                element && $(element).text('↗');
                EL.$div.find('i.laymd-tool-preview').show();
                EL.$body.removeAttr('style');
            } else {
                EL.$div.addClass('layui-laymd-full');
                element && $(element).text('↙');
                EL.$div.find('i.laymd-tool-preview').hide();
                EL.$body.attr('style', 'overflow: hidden;');
            }
        },
        preview: function (event, element, EL, params) {
            if (EL.$iframe.is(':visible')) {
                EL.$iframe.hide();
                element && $(element).removeClass('select');
            } else {
                EL.$iframe.show();
                element && $(element).addClass('select');
            }
        }
    };

    exports(MOD_NAME, {
        init: function (id, options) {
            return new MD(id, options);
        }
    });
});
