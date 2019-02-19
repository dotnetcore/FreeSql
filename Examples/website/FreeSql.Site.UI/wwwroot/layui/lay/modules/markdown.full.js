/**
 @Name：layui.markdown markdown编辑器
 @Author：kun
 @License：LGPL
 */
layui.define(['layer', 'form'], function (exports) {
    var $ = layui.jquery,
        _form = layui.form(),
        _layer = layui.layer,
        markdown = function () {
            //全局配置
            this.config = {
                //默认工具bar
                tools: [
                    'face', 'image', 'link', 'code', 'preview', 'help'
                ],
                height: 280 //默认高
            };
        };

    // markdown 解析器
    var _parser = new HyperDown;

    var tools = {
        face: '<span event="face" title="插入表情"><i class="iconfont" event="face">&#xe61d;</i>表情</span>',
        image: '<span event="image" title="插入图片"><i class="iconfont" event="image">&#xe680;</i>图片</span>',
        link: '<span event="link" title="插入链接"><i class="iconfont" event="link">&#xe6aa;</i>链接</span>',
        code: '<span event="code" title="插入代码"><i class="iconfont" event="code">&#xe6a1;</i>代码</span>',
        preview: '<span event="preview" title="预览"><i class="iconfont" event="preview">&#xe75b;</i>预览</span>'
    };

    // 建立编辑器
    markdown.prototype.build = function (id, settings) {
        var _settings = $.extend({}, this.config, (settings || {}));
        var image_upload_action = _settings.image_upload_action; // 上传图片的action
        var _tools = (function () {
            var _nodes = [];
            $.each(_settings.tools, function (item) {
                if (tools[_settings.tools[item]])
                    _nodes.push(tools[_settings.tools[item]]);
            });
            return _nodes.join('');
        })();

        var editor = $([
            '<div class="layui-markdown">',
            '  <div class="tools">' + _tools + '<span type="help" style="float: right; color: #afafaf"><small>markdown</small></span></div>',
            '  <textarea placeholder="请输入内容" name="markdown_content" v-model="markdown_content" debounce="500" style="min-height: ' + _settings.height + 'px"></textarea>',
            '</div>'].join('')
        );

        $('#' + id).empty().append(editor);

        // 表情
        $('#' + id).find("span[event='face']").click(function () {
            face($(this), function (img) {
                // insertTextarea(document.getElementsByName("markdown_content")[0], "@" + img.alt +"(" + img.src + ")")
                insertTextarea(document.getElementsByName("markdown_content")[0], "@" + img.alt);
            })
        });

        // 图片
        $('#' + id).find("span[event='image']").click(function () {
            // _layer.msg("开发中...", {shift: 6, time: 1000});
            var textarea = document.getElementsByName("markdown_content")[0];
            image($(this), image_upload_action, function (image) {
                insertTextarea(textarea, "![" + image.alt + "](" + image.src + ")");
            });
        });

        // 链接
        $('#' + id).find("span[event='link']").click(function () {
            var textarea = document.getElementsByName("markdown_content")[0];
            var text = textarea.value.substring(textarea.selectionStart, textarea.selectionEnd);
            link($(this), text, function (link) {
                var text = "";
                if (link.title && link.url) {
                    text = link.title ? "[" + link.title + "](" + link.url + ")" : link.url
                } else if (!link.title && link.url) {
                    text = link.url;
                }
                insertTextarea(textarea, text);
            });
        });

        // 插入代码
        $('#' + id).find("span[event='code']").click(function () {
            var textarea = document.getElementsByName("markdown_content")[0];
            code($(this), function (_code) {
                insertTextarea(textarea, "\n~~~lang-" + _code.lang + "\n" + _code.content + "\n" + "~~~" + "\n");
            });
        });

        // 预览
        $('#' + id).find("span[event='preview']").click(function () {
            var textarea = document.getElementsByName("markdown_content")[0];
            _layer.open({
                title: "预览", type: 1, area: ["100%", "100%"], btn: null, shadeClose: true, shade: 0.2,
                content: "<div class='layui-markdown' style='width: 1000px; margin: 5px auto 20px; padding: 25px 20px; background: #FBFBFB;'>" +
                                    _parser.makeHtml(textarea.value) + "</div>",
                success: function (layero, index) {
                    $(document).on('keydown', function (e) {
                        if (e.keyCode == 27)
                            _layer.close(index);
                    });
                    Prism.highlightAll();
                }
            });
        });
    };

    // 修改textarea文字
    var insertTextarea = function (textarea, str) {
        if (textarea) {
            var _value = textarea.value;
            textarea.value = _value.substring(0, textarea.selectionStart) + str + _value.substring(textarea.selectionEnd);
        }
    };

    // code
    var code = function(obj, callback) {
        return _layer.open({
            title: "插入代码：", type: 1, area: ["800px", "500px"], btn: null, shadeClose: true, shade: 0.2,
            content: "<div class='layui-form' style='padding: 20px;'>" +
                     "  <div class='layui-form-item' style='margin-left: -20px'>" +
                     "    <div class='layui-input-inline'>" +
                     "      <select name='lang'>" +
                     "        <option value=''>请选择语言</option>" +
                     "        <option value='javascript'>Javascript</option>" +
                     "        <option value='html'>HTML</option>" +
                     "        <option value='css'>CSS</option>" +
                     "        <option value='php'>PHP</option>" +
                     "        <option value='java'>Java</option>" +
                     "        <option value='ruby'>Ruby</option>" +
                     "        <option value='python'>Python</option>" +
                     "        <option value='csharp'>C#</option>" +
                     "        <option value='aspnet'>ASP.NET</option>" +
                     "        <option value='json'>JSON</option>" +
                     "        <option value='sql'>SQL</option>" +
                     "        <option value='markdown'>Markdown</option>" +
                     "      </select>" +
                     "    </div>" +
                     "  </div>" +
                     "  <div class='layui-form-item' style='margin-top: 10px'>" +
                     "    <textarea name='content' autofocus='true' " +
                     "style='height: 300px; width: 100%; line-height: 20px; padding: 8px; -webkit-box-sizing: border-box; border: 1px solid #ccc; box-shadow: 1px 1px 5px rgba(0,0,0,.1) inset; color: #333'></textarea>" +
                     "  </div>" +
                     "  <div style='margin-top: 20px; text-align: right'>" +
                     "    <button type='button' name='yes' class='layui-btn'><i class='iconfont'>&#xe7bd;</i>&ensp; 确&ensp;定 </button>" +
                     "    <button style='margin-left: 20px;' type='button' class='layui-btn layui-btn-primary' name='cancel'> 取消 </button>" +
                     "  </div>" +
                     "</div>",
            success: function (layero, index) {
                _form.render("select");
                layero.find("button[name='yes']").click(function () {
                    var _lang = layero.find("select[name='lang']");
                    var _content = layero.find("textarea[name='content']");
                    if (_lang.val() == "") {
                        _lang.addClass("layui-form-danger").val("").focus();
                        _layer.msg("请选择语言", {shift: 6, time: 800});
                    }
                    else if (_content.val() == "") {
                        _content.addClass("layui-form-danger").val("").focus();
                        _layer.msg("请输入代码", {shift: 6, time: 800});
                    } else {
                        callback({
                            lang: _lang.val(), content: _content.val()
                        });
                        _layer.close(index);
                    }
                });
                layero.find("button[name='cancel']").click(function () {
                    _layer.close(index);
                });
                $(document).on('keydown', function (e) {
                    if (e.keyCode == 27)
                        _layer.close(index);
                });
            }
        });
    };

    var image = function (obj, image_upload_action, callback) {
        return _layer.open({
            title: "插入图片：", type: 1, area: ["470px", "265px"], btn: null, shadeClose: true, shade: 0.2,
            content: "<div style='margin-top: 20px; margin-left: 16px'>" +
            "  <label class='layui-form-label' style='width: 100px !important; margin-left: -27px;'>图片描述</label>" +
            "  <div class='layui-input-block' style='margin-left: 90px !important'>" +
            "    <input type='text' name='text' class='layui-input' value='' autocomplete='off' style='width: 250px' />" +
            "  </div>" +
            "  <label class='layui-form-label required' style='width: 100px !important; margin-left: -27px; margin-top: 20px'>图片地址</label>" +
            "  <div class='layui-input-block' style='margin-left: 90px !important; margin-top: 20px'>" +
            "    <input type='url' name='url' autofocus class='layui-input' autocomplete='off' style='width: 250px; display: inline-block' />" +
                "<button style='margin-left: 5px; display: inline-block; color: #1AA094; border: 1px solid #1AA094; transform: translateY(-2px); font-size: 17px; padding: 0 10px' type='button' class='layui-btn layui-btn-primary' name='upload'>" +
            "      <i class='iconfont' style='position: relative; top: -3px; font-size: 18px;'>&#xe61f;</i> 上传</button>" +
            "    <form method='post' id='upload_image' enctype='multipart/form-data' action='" + image_upload_action + "'>" +
            "       <input type='file' name='file' accept='.jpg,.gif,.png,.bmp' style='display: none' /></form>" +
            "  </div>" +
            "  <div style='margin-top: 35px; text-align: right; padding-right: 30px;'>" +
            "    <button type='button' name='yes' class='layui-btn'><i class='iconfont'>&#xe7bd;</i>&ensp; 确&ensp;定 </button>" +
            "    <button style='margin-left: 20px;' type='button' class='layui-btn layui-btn-primary' name='cancel'> 取消 </button>" +
            "  </div>" +
            "</div>",
            success: function (layero, index) {
                layero.find("input[name='url']").focus();
                layero.find("button[name='upload']").click(function () {
                    if (image_upload_action == null || image_upload_action == '') {
                        _layer.msg("没有定义上传图片action地址！", {shift: 6, time: 1000});
                    } else {
                        layero.find("input[name='file']").click();
                    }
                });
                layero.find("input[name='file']").change(function () {
                    if ($(this).val() == '') return;
                    var load = _layer.load(8);
                    layero.find("#upload_image").ajaxSubmit({
                        success: function (data) {
                            _layer.close(load);
                            if (data.status == 0) {
                                layero.find("input[name='url']").val('');
                                _layer.alert(data.msg, {icon: 5, shade: 0.6});
                            } else {
                                layero.find("input[name='url']").val(data.src);
                            }
                        }
                    });
                });
                layero.find("button[name='yes']").click(function () {
                    var url = layero.find("input[name='url']");
                    if (url.val() == "" || url.val().match(/^((https|http)?:\/\/)?[^\s]+\.(png|jpg|jpeg|gif|svg|bmp)/gi  ) == null) {
                        url.addClass("layui-form-danger").val("").focus();
                        _layer.msg("请输入正确的图片地址", {shift: 6, time: 800});
                    } else {
                        callback({
                            alt: layero.find("input[name='text']").val(),
                            src: url.val()
                        });
                        _layer.close(index);
                    }
                });
                layero.find("button[name='cancel']").click(function () {
                    _layer.close(index);
                });
                $(document).on('keydown', function (e) {
                    if (e.keyCode == 27)
                        _layer.close(index);
                });
            }
        });
    };

    // 链接
    var link = function (obj, text, callback) {
        return _layer.open({
            title: "插入链接：", type: 1, area: ["400px", "260px"], btn: null, shadeClose: true, shade: 0.2,
            content: "<div style='margin-top: 20px; margin-left: 16px'>" +
                     "  <label class='layui-form-label' style='width: 100px !important; margin-left: -27px;'>链接文字</label>" +
                     "  <div class='layui-input-block' style='margin-left: 90px !important; width: 256px'>" +
                     "    <input type='text' name='text' class='layui-input' value='" + text + "' autocomplete='off' />" +
                     "  </div>" +
                     "  <label class='layui-form-label required' style='width: 100px !important; margin-left: -27px; margin-top: 20px'>链接</label>" +
                     "  <div class='layui-input-block' style='margin-left: 90px !important; width: 256px; margin-top: 20px'>" +
                     "    <input type='url' name='url' autofocus class='layui-input' autocomplete='off' />" +
                     "  </div>" +
                     "  <div style='margin-top: 35px; text-align: right; padding-right: 38px;'>" +
                     "    <button type='button' name='yes' class='layui-btn'><i class='iconfont'>&#xe7bd;</i>&ensp; 确&ensp;定 </button>" +
                     "    <button style='margin-left: 20px;' type='button' class='layui-btn layui-btn-primary' name='cancel'> 取消 </button>" +
                     "  </div>" +
                     "</div>",
            success: function (layero, index) {
                layero.find("input[name='url']").focus();
                layero.find("button[name='yes']").click(function () {
                    var url = layero.find("input[name='url']");
                    if (url.val() == "" || url.val().match(/^((https|http|ftp|rtsp|mms)?:\/\/)[^\s]+/g) == null) {
                        url.addClass("layui-form-danger").val("").focus();
                        _layer.msg("请输入正确的链接", {shift: 6, time: 800});
                    } else {
                        callback({
                            title: layero.find("input[name='text']").val(),
                            url: url.val()
                        });
                        _layer.close(index);
                    }
                });
                layero.find("button[name='cancel']").click(function () {
                    _layer.close(index);
                });
                $(document).on('keydown', function (e) {
                    if (e.keyCode == 27)
                        _layer.close(index);
                });
            }
        });
    };

    // 表情弹窗
    var face = function (obj, callback) {
        //表情库
        var faces = function () {
            var alt = ["[微笑]", "[嘻嘻]", "[哈哈]", "[可爱]", "[可怜]", "[挖鼻]", "[吃惊]", "[害羞]", "[挤眼]", "[闭嘴]",
                "[鄙视]", "[爱你]", "[泪]", "[偷笑]", "[亲亲]", "[生病]", "[太开心]", "[白眼]", "[右哼哼]", "[左哼哼]",
                "[嘘]", "[衰]", "[委屈]", "[吐]", "[哈欠]", "[抱抱]", "[怒]", "[疑问]", "[馋嘴]", "[拜拜]", "[思考]",
                "[汗]", "[困]", "[睡]", "[钱]", "[失望]", "[酷]", "[色]", "[哼]", "[鼓掌]", "[晕]", "[悲伤]", "[抓狂]",
                "[黑线]", "[阴险]", "[怒骂]", "[互粉]", "[心]", "[伤心]", "[猪头]", "[熊猫]", "[兔子]", "[ok]", "[耶]",
                "[good]", "[NO]", "[赞]", "[来]", "[弱]", "[草泥马]", "[神马]", "[囧]", "[浮云]", "[给力]", "[围观]",
                "[威武]", "[奥特曼]", "[礼物]", "[钟]", "[话筒]", "[蜡烛]", "[蛋糕]"], arr = {};
            layui.each(alt, function (index, item) {
                arr[item] = layui.cache.dir + 'images/face/' + index + '.gif';
            });
            return arr;
        }();
        face.hide = face.hide || function (e) {
                if ($(e.target).attr('event') !== 'face') {
                    _layer.close(face.index);
                }
            };
        return face.index = _layer.tips((function () {
            var content = [];
            layui.each(faces, function (key, item) {
                content.push('<li title="' + key + '"><img src="' + item + '" alt="' + key + '"></li>');
            });
            return '<ul class="layui-clear">' + content.join('') + '</ul>';
        })(), obj, {
            tips: 3, time: 0, skin: 'layui-box layui-util-face', maxWidth: 500,
            success: function (layero, index) {
                layero.css({
                    marginTop: -4, marginLeft: -10
                }).find('.layui-clear > li').on('click', function () {
                    callback && callback({
                        src: faces[this.title], alt: this.title
                    });
                    layer.close(index);
                });
                $(document).off('click', face.hide).on('click', face.hide);
            }
        });
    };

    exports('markdown', new markdown());
});







// layui.define('layer', function(exports){
//     "use strict";
//
//     var $ = layui.jquery,
//         layer = layui.layer,
//         MOD_NAME = 'markdown', THIS = 'layui-this', SHOW = 'layui-show', ABLED = 'layui-disabled',
//         MARKDOWN = function () {
//             var that = this;
//             that.index = 0;
//
//             //全局配置
//             that.config = {
//                 //默认工具bar
//                 tool: [
//                     'face', 'image', 'link', 'code', 'preview'
//                 ],
//                 hideTool: [],
//                 height: 280 //默认高
//             };
//         };
//
//     //全局设置
//     MARKDOWN.prototype.set = function(options){
//         var that = this;
//         $.extend(true, that.config, options);
//         return that;
//     };
//
//     //事件监听
//     MARKDOWN.prototype.on = function(events, callback){
//         return layui.onevent(MOD_NAME, events, callback);
//     };
//
//     //建立编辑器
//     MARKDOWN.prototype.build = function (id, settings) {
//         settings = settings || {};
//
//         var that = this,
//             config = that.config,
//             ELEM = 'layui-markdown',
//             textArea = $('#' + id),
//             name = 'LAY_markdown_' + (++that.index),
//             haveBuild = textArea.next('.' + ELEM),
//             set = $.extend({}, config, settings),
//             tool = function () {
//                 var node = [], hideTools = {};
//                 layui.each(set.hideTool, function (_, item) {
//                     hideTools[item] = true;
//                 });
//                 layui.each(set.tool, function (_, item) {
//                     if (tools[item] && !hideTools[item]) {
//                         node.push(tools[item]);
//                     }
//                 });
//                 return node.join('');
//             }()
//
//
//             , editor = $(['<div class="' + ELEM + '">'
//             , '<div class="layui-unselect layui-markdown-tool">' + tool + '</div>'
//             , '<div class="layui-markdown-iframe">'
//             , '<iframe id="' + name + '" name="' + name + '" textarea="' + id + '" frameborder="0"></iframe>'
//             , '</div>'
//             , '</div>'].join(''))
//
//         //编辑器不兼容ie8以下
//         if (device.ie && device.ie < 8) {
//             return textArea.removeClass('layui-hide').addClass(SHOW);
//         }
//
//         haveBuild[0] && (haveBuild.remove());
//
//         setIframe.call(that, editor, textArea[0], set)
//         textArea.addClass('layui-hide').after(editor);
//
//         return that.index;
//     };
//
//     //获得编辑器中内容
//     MARKDOWN.prototype.getContent = function(index){
//         var iframeWin = getWin(index);
//         if(!iframeWin[0]) return;
//         return toLower(iframeWin[0].document.body.innerHTML);
//     };
//
//     //获得编辑器中纯文本内容
//     MARKDOWN.prototype.getText = function(index){
//         var iframeWin = getWin(index);
//         if(!iframeWin[0]) return;
//         return $(iframeWin[0].document.body).text();
//     };
//
//     //将编辑器内容同步到textarea（一般用于异步提交时）
//     MARKDOWN.prototype.sync = function(index){
//         var iframeWin = getWin(index);
//         if(!iframeWin[0]) return;
//         var textarea = $('#'+iframeWin[1].attr('textarea'));
//         textarea.val(toLower(iframeWin[0].document.body.innerHTML));
//     };
//
//     //获取编辑器选中内容
//     MARKDOWN.prototype.getSelection = function(index){
//         var iframeWin = getWin(index);
//         if(!iframeWin[0]) return;
//         var range = Range(iframeWin[0].document);
//         return document.selection ? range.text : range.toString();
//     };
//
//     //iframe初始化
//     var setIframe = function(editor, textArea, set){
//             var that = this, iframe = editor.find('iframe');
//
//             iframe.css({
//                 height: set.height
//             }).on('load', function(){
//                 var conts = iframe.contents()
//                     ,iframeWin = iframe.prop('contentWindow')
//                     ,head = conts.find('head')
//                     ,style = $(['<style>'
//                     ,'*{margin: 0; padding: 0;}'
//                     ,'body{padding: 10px; line-height: 20px; overflow-x: hidden; word-wrap: break-word; font: 14px Helvetica Neue,Helvetica,PingFang SC,Microsoft YaHei,Tahoma,Arial,sans-serif; -webkit-box-sizing: border-box !important; -moz-box-sizing: border-box !important; box-sizing: border-box !important;}'
//                     ,'a{color:#01AAED; text-decoration:none;}a:hover{color:#c00}'
//                     ,'p{margin-bottom: 10px;}'
//                     ,'img{display: inline-block; border: none; vertical-align: middle;}'
//                     ,'pre{margin: 10px 0; padding: 10px; line-height: 20px; border: 1px solid #ddd; border-left-width: 6px; background-color: #F2F2F2; color: #333; font-family: Courier New; font-size: 12px;}'
//                     ,'</style>'].join(''))
//                     ,body = conts.find('body');
//
//                 head.append(style);
//                 body.attr('contenteditable', 'true').css({
//                     'min-height': set.height
//                 }).html(textArea.value||'');
//
//                 hotkey.apply(that, [iframeWin, iframe, textArea, set]); //快捷键处理
//                 toolActive.call(that, iframeWin, editor, set); //触发工具
//
//             });
//         }
//
//         //获得iframe窗口对象
//         ,getWin = function(index){
//             var iframe = $('#LAY_markdown_'+ index)
//                 ,iframeWin = iframe.prop('contentWindow');
//             return [iframeWin, iframe];
//         }
//
//         //IE8下将标签处理成小写
//         ,toLower = function(html){
//             if(device.ie == 8){
//                 html = html.replace(/<.+>/g, function(str){
//                     return str.toLowerCase();
//                 });
//             }
//             return html;
//         }
//
//         //快捷键处理
//         ,hotkey = function(iframeWin, iframe, textArea, set){
//             var iframeDOM = iframeWin.document, body = $(iframeDOM.body);
//             body.on('keydown', function(e){
//                 var keycode = e.keyCode;
//                 //处理回车
//                 if(keycode === 13){
//                     var range = Range(iframeDOM);
//                     var container = getContainer(range)
//                         ,parentNode = container.parentNode;
//
//                     if(parentNode.tagName.toLowerCase() === 'pre'){
//                         if(e.shiftKey) return
//                         layer.msg('请暂时用shift+enter');
//                         return false;
//                     }
//                     iframeDOM.execCommand('formatBlock', false, '<p>');
//                 }
//             });
//
//             //给textarea同步内容
//             $(textArea).parents('form').on('submit', function(){
//                 var html = body.html();
//                 //IE8下将标签处理成小写
//                 if(device.ie == 8){
//                     html = html.replace(/<.+>/g, function(str){
//                         return str.toLowerCase();
//                     });
//                 }
//                 textArea.value = html;
//             });
//
//             //处理粘贴
//             body.on('paste', function(e){
//                 iframeDOM.execCommand('formatBlock', false, '<p>');
//                 setTimeout(function(){
//                     filter.call(iframeWin, body);
//                     textArea.value = body.html();
//                 }, 100);
//             });
//         }
//
//         //标签过滤
//         ,filter = function(body){
//             var iframeWin = this
//                 ,iframeDOM = iframeWin.document;
//
//             //清除影响版面的css属性
//             body.find('*[style]').each(function(){
//                 var textAlign = this.style.textAlign;
//                 this.removeAttribute('style');
//                 $(this).css({
//                     'text-align': textAlign || ''
//                 })
//             });
//
//             //修饰表格
//             body.find('table').addClass('layui-table');
//
//             //移除不安全的标签
//             body.find('script,link').remove();
//         }
//
//         //Range对象兼容性处理
//         ,Range = function(iframeDOM){
//             return iframeDOM.selection
//                 ? iframeDOM.selection.createRange()
//                 : iframeDOM.getSelection().getRangeAt(0);
//         }
//
//         //当前Range对象的endContainer兼容性处理
//         ,getContainer = function(range){
//             return range.endContainer || range.parentElement().childNodes[0]
//         }
//
//         //在选区插入内联元素
//         ,insertInline = function(tagName, attr, range){
//             var iframeDOM = this.document
//                 ,elem = document.createElement(tagName)
//             for(var key in attr){
//                 elem.setAttribute(key, attr[key]);
//             }
//             elem.removeAttribute('text');
//
//             if(iframeDOM.selection){ //IE
//                 var text = range.text || attr.text;
//                 if(tagName === 'a' && !text) return;
//                 if(text){
//                     elem.innerHTML = text;
//                 }
//                 range.pasteHTML($(elem).prop('outerHTML'));
//                 range.select();
//             } else { //非IE
//                 var text = range.toString() || attr.text;
//                 if(tagName === 'a' && !text) return;
//                 if(text){
//                     elem.innerHTML = text;
//                 }
//                 range.deleteContents();
//                 range.insertNode(elem);
//             }
//         }
//
//         //工具选中
//         ,toolCheck = function(tools, othis){
//             var iframeDOM = this.document
//                 ,CHECK = 'layedit-tool-active'
//                 ,container = getContainer(Range(iframeDOM))
//                 ,item = function(type){
//                 return tools.find('.layedit-tool-'+type)
//             }
//
//             if(othis){
//                 othis[othis.hasClass(CHECK) ? 'removeClass' : 'addClass'](CHECK);
//             }
//
//             tools.find('>i').removeClass(CHECK);
//             item('unlink').addClass(ABLED);
//
//             $(container).parents().each(function(){
//                 var tagName = this.tagName.toLowerCase()
//                     ,textAlign = this.style.textAlign;
//
//                 //文字
//                 if(tagName === 'b' || tagName === 'strong'){
//                     item('b').addClass(CHECK)
//                 }
//                 if(tagName === 'i' || tagName === 'em'){
//                     item('i').addClass(CHECK)
//                 }
//                 if(tagName === 'u'){
//                     item('u').addClass(CHECK)
//                 }
//                 if(tagName === 'strike'){
//                     item('d').addClass(CHECK)
//                 }
//
//                 //对齐
//                 if(tagName === 'p'){
//                     if(textAlign === 'center'){
//                         item('center').addClass(CHECK);
//                     } else if(textAlign === 'right'){
//                         item('right').addClass(CHECK);
//                     } else {
//                         item('left').addClass(CHECK);
//                     }
//                 }
//
//                 //超链接
//                 if(tagName === 'a'){
//                     item('link').addClass(CHECK);
//                     item('unlink').removeClass(ABLED);
//                 }
//             });
//         }
//
//         //触发工具
//         ,toolActive = function(iframeWin, editor, set){
//             var iframeDOM = iframeWin.document
//                 ,body = $(iframeDOM.body)
//                 ,toolEvent = {
//                 //超链接
//                 link: function(range){
//                     var container = getContainer(range)
//                         ,parentNode = $(container).parent();
//
//                     link.call(body, {
//                         href: parentNode.attr('href')
//                         ,target: parentNode.attr('target')
//                     }, function(field){
//                         var parent = parentNode[0];
//                         if(parent.tagName === 'A'){
//                             parent.href = field.url;
//                         } else {
//                             insertInline.call(iframeWin, 'a', {
//                                 target: field.target
//                                 ,href: field.url
//                                 ,text: field.url
//                             }, range);
//                         }
//                     });
//                 }
//                 //清除超链接
//                 ,unlink: function(range){
//                     iframeDOM.execCommand('unlink');
//                 }
//                 //表情
//                 ,face: function(range){
//                     face.call(this, function(img){
//                         insertInline.call(iframeWin, 'img', {
//                             src: img.src
//                             ,alt: img.alt
//                         }, range);
//                     });
//                 }
//                 //图片
//                 ,image: function(range){
//                     var that = this;
//                     layui.use('upload', function(upload){
//                         var uploadImage = set.uploadImage || {};
//                         upload({
//                             url: uploadImage.url
//                             ,method: uploadImage.type
//                             ,elem: $(that).find('input')[0]
//                             ,unwrap: true
//                             ,success: function(res){
//                                 if(res.code == 0){
//                                     res.data = res.data || {};
//                                     insertInline.call(iframeWin, 'img', {
//                                         src: res.data.src
//                                         ,alt: res.data.title
//                                     }, range);
//                                 } else {
//                                     layer.msg(res.msg||'上传失败');
//                                 }
//                             }
//                         });
//                     });
//                 }
//                 //插入代码
//                 ,code: function(range){
//                     code.call(body, function(pre){
//                         insertInline.call(iframeWin, 'pre', {
//                             text: pre.code
//                             ,'lay-lang': pre.lang
//                         }, range);
//                     });
//                 }
//                 //帮助
//                 ,help: function(){
//                     layer.open({
//                         type: 2
//                         ,title: '帮助'
//                         ,area: ['600px', '380px']
//                         ,shadeClose: true
//                         ,shade: 0.1
//                         ,skin: 'layui-layer-msg'
//                         ,content: ['http://www.layui.com/about/layedit/help.html', 'no']
//                     });
//                 }
//             }
//                 ,tools = editor.find('.layui-markdown-tool')
//
//                 ,click = function(){
//                 var othis = $(this)
//                     ,events = othis.attr('layedit-event')
//                     ,command = othis.attr('lay-command');
//
//                 if(othis.hasClass(ABLED)) return;
//
//                 body.focus();
//
//                 var range = Range(iframeDOM)
//                     ,container = range.commonAncestorContainer
//
//                 if(command){
//                     iframeDOM.execCommand(command);
//                     if(/justifyLeft|justifyCenter|justifyRight/.test(command)){
//                         iframeDOM.execCommand('formatBlock', false, '<p>');
//                     }
//                     setTimeout(function(){
//                         body.focus();
//                     }, 10);
//                 } else {
//                     toolEvent[events] && toolEvent[events].call(this, range);
//                 }
//                 toolCheck.call(iframeWin, tools, othis);
//             }
//
//                 ,isClick = /image/
//
//             tools.find('>i').on('mousedown', function(){
//                 var othis = $(this)
//                     ,events = othis.attr('layedit-event');
//                 if(isClick.test(events)) return;
//                 click.call(this)
//             }).on('click', function(){
//                 var othis = $(this)
//                     ,events = othis.attr('layedit-event');
//                 if(!isClick.test(events)) return;
//                 click.call(this)
//             });
//
//             //触发内容区域
//             body.on('click', function(){
//                 toolCheck.call(iframeWin, tools);
//                 layer.close(face.index);
//             });
//         }
//
//         //超链接面板
//         ,link = function(options, callback){
//             var body = this, index = layer.open({
//                 type: 1
//                 ,id: 'LAY_markdown_link'
//                 ,area: '350px'
//                 ,shade: 0.05
//                 ,shadeClose: true
//                 ,moveType: 1
//                 ,title: '超链接'
//                 ,skin: 'layui-layer-msg'
//                 ,content: ['<ul class="layui-form" style="margin: 15px;">'
//                     ,'<li class="layui-form-item">'
//                     ,'<label class="layui-form-label" style="width: 60px;">URL</label>'
//                     ,'<div class="layui-input-block" style="margin-left: 90px">'
//                     ,'<input name="url" lay-verify="url" value="'+ (options.href||'') +'" autofocus="true" autocomplete="off" class="layui-input">'
//                     ,'</div>'
//                     ,'</li>'
//                     ,'<li class="layui-form-item">'
//                     ,'<label class="layui-form-label" style="width: 60px;">打开方式</label>'
//                     ,'<div class="layui-input-block" style="margin-left: 90px">'
//                     ,'<input type="radio" name="target" value="_self" class="layui-input" title="当前窗口"'
//                     + ((options.target==='_self' || !options.target) ? 'checked' : '') +'>'
//                     ,'<input type="radio" name="target" value="_blank" class="layui-input" title="新窗口" '
//                     + (options.target==='_blank' ? 'checked' : '') +'>'
//                     ,'</div>'
//                     ,'</li>'
//                     ,'<li class="layui-form-item" style="text-align: center;">'
//                     ,'<button type="button" lay-submit lay-filter="layedit-link-yes" class="layui-btn"> 确定 </button>'
//                     ,'<button style="margin-left: 20px;" type="button" class="layui-btn layui-btn-primary"> 取消 </button>'
//                     ,'</li>'
//                     ,'</ul>'].join('')
//                 ,success: function(layero, index){
//                     var eventFilter = 'submit(layedit-link-yes)';
//                     form.render('radio');
//                     layero.find('.layui-btn-primary').on('click', function(){
//                         layer.close(index);
//                         body.focus();
//                     });
//                     form.on(eventFilter, function(data){
//                         layer.close(link.index);
//                         callback && callback(data.field);
//                     });
//                 }
//             });
//             link.index = index;
//         }
//
//         //表情面板
//         ,face = function(callback){
//             //表情库
//             var faces = function(){
//                 var alt = ["[微笑]", "[嘻嘻]", "[哈哈]", "[可爱]", "[可怜]", "[挖鼻]", "[吃惊]", "[害羞]", "[挤眼]", "[闭嘴]", "[鄙视]", "[爱你]", "[泪]", "[偷笑]", "[亲亲]", "[生病]", "[太开心]", "[白眼]", "[右哼哼]", "[左哼哼]", "[嘘]", "[衰]", "[委屈]", "[吐]", "[哈欠]", "[抱抱]", "[怒]", "[疑问]", "[馋嘴]", "[拜拜]", "[思考]", "[汗]", "[困]", "[睡]", "[钱]", "[失望]", "[酷]", "[色]", "[哼]", "[鼓掌]", "[晕]", "[悲伤]", "[抓狂]", "[黑线]", "[阴险]", "[怒骂]", "[互粉]", "[心]", "[伤心]", "[猪头]", "[熊猫]", "[兔子]", "[ok]", "[耶]", "[good]", "[NO]", "[赞]", "[来]", "[弱]", "[草泥马]", "[神马]", "[囧]", "[浮云]", "[给力]", "[围观]", "[威武]", "[奥特曼]", "[礼物]", "[钟]", "[话筒]", "[蜡烛]", "[蛋糕]"], arr = {};
//                 layui.each(alt, function(index, item){
//                     arr[item] = layui.cache.dir + 'images/face/'+ index + '.gif';
//                 });
//                 return arr;
//             }();
//             face.hide = face.hide || function(e){
//                     if($(e.target).attr('layedit-event') !== 'face'){
//                         layer.close(face.index);
//                     }
//                 }
//             return face.index = layer.tips(function(){
//                 var content = [];
//                 layui.each(faces, function(key, item){
//                     content.push('<li title="'+ key +'"><img src="'+ item +'" alt="'+ key +'"></li>');
//                 });
//                 return '<ul class="layui-clear">' + content.join('') + '</ul>';
//             }(), this, {
//                 tips: 1
//                 ,time: 0
//                 ,skin: 'layui-box layui-util-face'
//                 ,maxWidth: 500
//                 ,success: function(layero, index){
//                     layero.css({
//                         marginTop: -4
//                         ,marginLeft: -10
//                     }).find('.layui-clear>li').on('click', function(){
//                         callback && callback({
//                             src: faces[this.title]
//                             ,alt: this.title
//                         });
//                         layer.close(index);
//                     });
//                     $(document).off('click', face.hide).on('click', face.hide);
//                 }
//             });
//         }
//
//         //插入代码面板
//         ,code = function(callback){
//             var body = this, index = layer.open({
//                 type: 1
//                 ,id: 'LAY_markdown_code'
//                 ,area: '550px'
//                 ,shade: 0.05
//                 ,shadeClose: true
//                 ,moveType: 1
//                 ,title: '插入代码'
//                 ,skin: 'layui-layer-msg'
//                 ,content: ['<ul class="layui-form layui-form-pane" style="margin: 15px;">'
//                     ,'<li class="layui-form-item">'
//                     ,'<label class="layui-form-label">请选择语言</label>'
//                     ,'<div class="layui-input-block">'
//                     ,'<select name="lang">'
//                     ,'<option value="JavaScript">JavaScript</option>'
//                     ,'<option value="HTML">HTML</option>'
//                     ,'<option value="CSS">CSS</option>'
//                     ,'<option value="Java">Java</option>'
//                     ,'<option value="PHP">PHP</option>'
//                     ,'<option value="C#">C#</option>'
//                     ,'<option value="Python">Python</option>'
//                     ,'<option value="Ruby">Ruby</option>'
//                     ,'<option value="Go">Go</option>'
//                     ,'</select>'
//                     ,'</div>'
//                     ,'</li>'
//                     ,'<li class="layui-form-item layui-form-text">'
//                     ,'<label class="layui-form-label">代码</label>'
//                     ,'<div class="layui-input-block">'
//                     ,'<textarea name="code" lay-verify="required" autofocus="true" class="layui-textarea" style="height: 200px;"></textarea>'
//                     ,'</div>'
//                     ,'</li>'
//                     ,'<li class="layui-form-item" style="text-align: center;">'
//                     ,'<button type="button" lay-submit lay-filter="layedit-code-yes" class="layui-btn"> 确定 </button>'
//                     ,'<button style="margin-left: 20px;" type="button" class="layui-btn layui-btn-primary"> 取消 </button>'
//                     ,'</li>'
//                     ,'</ul>'].join('')
//                 ,success: function(layero, index){
//                     var eventFilter = 'submit(layedit-code-yes)';
//                     form.render('select');
//                     layero.find('.layui-btn-primary').on('click', function(){
//                         layer.close(index);
//                         body.focus();
//                     });
//                     form.on(eventFilter, function(data){
//                         layer.close(code.index);
//                         callback && callback(data.field);
//                     });
//                 }
//             });
//             code.index = index;
//         }
//
//         //全部工具
//         ,tools = {
//             face: '<i class="layui-icon layedit-tool-face" title="表情" layedit-event="face"">&#xe650;</i>',
//
//             html: '<i class="layui-icon layedit-tool-html" title="HTML源代码" lay-command="html" layedit-event="html"">&#xe64b;</i><span class="layedit-tool-mid"></span>'
//             ,strong: '<i class="layui-icon layedit-tool-b" title="加粗" lay-command="Bold" layedit-event="b"">&#xe62b;</i>'
//             ,italic: '<i class="layui-icon layedit-tool-i" title="斜体" lay-command="italic" layedit-event="i"">&#xe644;</i>'
//             ,underline: '<i class="layui-icon layedit-tool-u" title="下划线" lay-command="underline" layedit-event="u"">&#xe646;</i>'
//             ,del: '<i class="layui-icon layedit-tool-d" title="删除线" lay-command="strikeThrough" layedit-event="d"">&#xe64f;</i>'
//
//             ,'|': '<span class="layedit-tool-mid"></span>'
//
//             ,left: '<i class="layui-icon layedit-tool-left" title="左对齐" lay-command="justifyLeft" layedit-event="left"">&#xe649;</i>'
//             ,center: '<i class="layui-icon layedit-tool-center" title="居中对齐" lay-command="justifyCenter" layedit-event="center"">&#xe647;</i>'
//             ,right: '<i class="layui-icon layedit-tool-right" title="右对齐" lay-command="justifyRight" layedit-event="right"">&#xe648;</i>'
//             ,link: '<i class="layui-icon layedit-tool-link" title="插入链接" layedit-event="link"">&#xe64c;</i>'
//             ,unlink: '<i class="layui-icon layedit-tool-unlink layui-disabled" title="清除链接" lay-command="unlink" layedit-event="unlink"">&#xe64d;</i>'
//             // ,face: '<i class="layui-icon layedit-tool-face" title="表情" layedit-event="face"">&#xe650;</i>'
//             ,image: '<i class="layui-icon layedit-tool-image" title="图片" layedit-event="image">&#xe64a;<input type="file" name="file"></i>'
//             ,code: '<i class="layui-icon layedit-tool-code" title="插入代码" layedit-event="code">&#xe64e;</i>'
//
//             ,help: '<i class="layui-icon layedit-tool-help" title="帮助" layedit-event="help">&#xe607;</i>'
//         }
//
//         ,edit = new MARKDOWN();
//
//     exports(MOD_NAME, edit);
// });