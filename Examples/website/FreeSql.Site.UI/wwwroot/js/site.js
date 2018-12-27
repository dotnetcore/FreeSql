/**

 layui官网

*/

layui.define(['code', 'element', 'table', 'util'], function (exports) {
    var $ = layui.jquery
        , element = layui.element
        , layer = layui.layer
        , form = layui.form
        , util = layui.util
        , device = layui.device()

        , $win = $(window), $body = $('body');


    //阻止IE7以下访问
    if (device.ie && device.ie < 8) {
        layer.alert('Layui最低支持ie8，您当前使用的是古老的 IE' + device.ie + '，你丫的肯定不是程序猿！');
    }

    var home = $('#LAY_home');


    layer.ready(function () {
        var local = layui.data('layui');

        //升级提示
        if (local.version && local.version !== layui.v) {
            layer.open({
                type: 1
                , title: '更新提示' //不显示标题栏
                , closeBtn: false
                , area: '300px;'
                , shade: false
                , offset: 'b'
                , id: 'LAY_updateNotice' //设定一个id，防止重复弹出
                , btn: ['更新日志', '朕不想升']
                , btnAlign: 'c'
                , moveType: 1 //拖拽模式，0或者1
                , content: ['<div class="layui-text">'
                    , 'layui 已更新到：<strong style="padding-right: 10px; color: #fff;">v' + layui.v + '</strong> <br>请注意升级！'
                    , '</div>'].join('')
                , skin: 'layui-layer-notice'
                , yes: function (index) {
                    layer.close(index);
                    setTimeout(function () {
                        location.href = '/doc/base/changelog.html';
                    }, 500);
                }
                , end: function () {
                    layui.data('layui', {
                        key: 'version'
                        , value: layui.v
                    });
                }
            });
        }
        layui.data('layui', {
            key: 'version'
            , value: layui.v
        });


        //公告
        ; !function () {
            return layui.data('layui', {
                key: 'notice_20180530'
                , remove: true
            });

            if (local.notice_20180530 && new Date().getTime() - local.notice_20180530 < 1000 * 60 * 60 * 24 * 5) {
                return;
            };

            layer.open({
                type: 1
                , title: 'layui 官方通用后台管理模板'
                , closeBtn: false
                , area: ['300px', '280px']
                , shade: false
                //,offset: 'c'
                , id: 'LAY_Notice' //设定一个id，防止重复弹出
                , btn: ['前往围观', '朕不想看']
                , btnAlign: 'b'
                , moveType: 1 //拖拽模式，0或者1
                , resize: false
                , content: ['<div style="padding: 15px; text-align: center; background-color: #e2e2e2;">'
                    , '<a href="/admin/std/dist/views/" target="_blank"><img src="//cdn.layui.com/upload/2018_5/168_1527691799254_76462.jpg" alt="layuiAdmin" style="width: 100%; height:149.78px;"></a>'
                    , '</div>'].join('')
                , success: function (layero, index) {
                    var btn = layero.find('.layui-layer-btn');
                    btn.find('.layui-layer-btn0').attr({
                        href: '/admin/std/dist/views/'
                        , target: '_blank'
                    });

                    layero.find('a').on('click', function () {
                        layer.close(index);
                    });
                }
                , end: function () {
                    layui.data('layui', {
                        key: 'notice_20180530'
                        , value: new Date().getTime()
                    });
                }
            });
        }();

    });

    ; !function () {
        var elemComponentSelect = $(['<select lay-search lay-filter="component">'
            , '<option value="">搜索组件或模块</option>'
            , '<option value="element/layout.html">grid 栅格布局</option>'
            , '<option value="element/layout.html#admin">admin 后台布局</option>'
            , '<option value="element/color.html">color 颜色</option>'
            , '<option value="element/icon.html">iconfont 字体图标</option>'
            , '<option value="element/anim.html">animation 动画</option>'
            , '<option value="element/button.html">button 按钮</option>'
            , '<option value="element/form.html">form 表单组</option>'
            , '<option value="element/form.html#input">input 输入框</option>'
            , '<option value="element/form.html#select">select 下拉选择框</option>'
            , '<option value="element/form.html#checkbox">checkbox 复选框</option>'
            , '<option value="element/form.html#switch">switch 开关</option>'
            , '<option value="element/form.html#radio">radio 单选框</option>'
            , '<option value="element/form.html#textarea">textarea 文本域</option>'
            , '<option value="element/nav.html">nav 导航菜单</option>'
            , '<option value="element/nav.html#breadcrumb">breadcrumb 面包屑</option>'
            , '<option value="element/tab.html">tabs 选项卡</option>'
            , '<option value="element/progress.html">progress 进度条</option>'
            , '<option value="element/collapse.html">collapse 折叠面板/手风琴</option>'
            , '<option value="element/table.html">table 表格元素</option>'
            , '<option value="element/badge.html">badge 徽章</option>'
            , '<option value="element/timeline.html">timeline 时间线</option>'
            , '<option value="element/auxiliar.html#blockquote">blockquote 引用块</option>'
            , '<option value="element/auxiliar.html#fieldset">fieldset 字段集</option>'
            , '<option value="element/auxiliar.html#hr">hr 分割线</option>'

            , '<option value="modules/layer.html">layer 弹出层/弹窗综合</option>'
            , '<option value="modules/laydate.html">laydate 日期时间选择器</option>'
            , '<option value="modules/layim.html">layim 即时通讯/聊天</option>'
            , '<option value="modules/laypage.html">laypage 分页</option>'
            , '<option value="modules/laytpl.html">laytpl 模板引擎</option>'
            , '<option value="modules/form.html">form 表单模块</option>'
            , '<option value="modules/table.html">table 数据表格</option>'
            , '<option value="modules/upload.html">upload 文件/图片上传</option>'
            , '<option value="modules/element.html">element 常用元素操作</option>'
            , '<option value="modules/rate.html">rate 评分</option>'
            , '<option value="modules/colorpicker.html">colorpicker 颜色选择器</option>'
            , '<option value="modules/slider.html">slider 滑块</option>'
            , '<option value="modules/carousel.html">carousel 轮播/跑马灯</option>'
            , '<option value="modules/layedit.html">layedit 富文本编辑器</option>'
            , '<option value="modules/tree.html">tree 树形菜单</option>'
            , '<option value="modules/flow.html">flow 信息流/图片懒加载</option>'
            , '<option value="modules/util.html">util 工具集</option>'
            , '<option value="modules/code.html">code 代码修饰</option>'
            , '</select>'].join(''));

        $('.component').append(elemComponentSelect);
        form.render('select', 'LAY-site-header-component');

        //搜索组件
        form.on('select(component)', function (data) {
            var value = data.value;
            location.href = '/doc/' + value;
        });
    }();


    //点击事件
    var events = {
        //联系方式
        contactInfo: function () {
            layer.alert('<div class="layui-text">如有合作意向，可联系：<br>邮箱：xianxin@layui-inc.com</div>', {
                title: '联系'
                , btn: false
                , shadeClose: true
            });
        }
    }

    $body.on('click', '*[site-event]', function () {
        var othis = $(this)
            , attrEvent = othis.attr('site-event');
        events[attrEvent] && events[attrEvent].call(this, othis);
    });

    //切换版本
    form.on('select(tabVersion)', function (data) {
        var value = data.value;
        location.href = value === 'new' ? '/' : ('/' + value + '/doc/');
    });


    //首页banner
    setTimeout(function () {
        $('.site-zfj').addClass('site-zfj-anim');
        setTimeout(function () {
            $('.site-desc').addClass('site-desc-anim')
        }, 5000)
    }, 100);


    //数字前置补零
    var digit = function (num, length, end) {
        var str = '';
        num = String(num);
        length = length || 2;
        for (var i = num.length; i < length; i++) {
            str += '0';
        }
        return num < Math.pow(10, length) ? str + (num | 0) : num;
    };


    //下载倒计时
    var setCountdown = $('#setCountdown');
    if ($('#setCountdown')[0]) {
        $.get('/api/getTime', function (res) {
            util.countdown(new Date(2017, 7, 21, 8, 30, 0), new Date(res.time), function (date, serverTime, timer) {
                var str = digit(date[1]) + ':' + digit(date[2]) + ':' + digit(date[3]);
                setCountdown.children('span').html(str);
            });
        }, 'jsonp');
    }



    for (var i = 0; i < $('.adsbygoogle').length; i++) {
        (adsbygoogle = window.adsbygoogle || []).push({});
    }


    //展示当前版本
    $('.site-showv').html(layui.v);

    ////获取下载数
    //$.get('//fly.layui.com/api/handle?id=10&type=find', function (res) {
    //    $('.site-showdowns').html(res.number);
    //}, 'jsonp');

    ////记录下载
    //$('.site-down').on('click', function () {
    //    $.get('//fly.layui.com/api/handle?id=10', function () { }, 'jsonp');
    //});

    //获取Github数据
    var getStars = $('#getStars');
    if (getStars[0]) {
        $.get('https://api.github.com/repos/2881099/FreeSql', function (res) {
            getStars.html(res.stargazers_count);
        }, 'json');
    }

    //固定Bar
    if (global.pageType !== 'demo') {
        util.fixbar({
            bar1: true
            , click: function (type) {
                if (type === 'bar1') {
                    location.href = '//fly.layui.com/';
                }
            }
        });
    }

    //窗口scroll
    ; !function () {
        var main = $('.site-tree').parent(), scroll = function () {
            var stop = $(window).scrollTop();

            if ($(window).width() <= 750) return;
            var bottom = $('.footer').offset().top - $(window).height();
            if (stop > 211 && stop < bottom) {
                if (!main.hasClass('site-fix')) {
                    main.addClass('site-fix');
                }
                if (main.hasClass('site-fix-footer')) {
                    main.removeClass('site-fix-footer');
                }
            } else if (stop >= bottom) {
                if (!main.hasClass('site-fix-footer')) {
                    main.addClass('site-fix site-fix-footer');
                }
            } else {
                if (main.hasClass('site-fix')) {
                    main.removeClass('site-fix').removeClass('site-fix-footer');
                }
            }
            stop = null;
        };
        scroll();
        $(window).on('scroll', scroll);
    }();

    //示例页面滚动
    $('.site-demo-body').on('scroll', function () {
        var elemDate = $('.layui-laydate,.layui-colorpicker-main')
            , elemTips = $('.layui-table-tips');
        if (elemDate[0]) {
            elemDate.each(function () {
                var othis = $(this);
                if (!othis.hasClass('layui-laydate-static')) {
                    othis.remove();
                }
            });
            $('input').blur();
        }
        if (elemTips[0]) elemTips.remove();

        if ($('.layui-layer')[0]) {
            layer.closeAll('tips');
        }
    });

    //代码修饰
    layui.code({
        elem: 'pre'
    });

    //目录
    var siteDir = $('.site-dir');
    if (siteDir[0] && $(window).width() > 750) {
        layer.ready(function () {
            layer.open({
                type: 1
                , content: siteDir
                , skin: 'layui-layer-dir'
                , area: 'auto'
                , maxHeight: $(window).height() - 300
                , title: '目录'
                //,closeBtn: false
                , offset: 'r'
                , shade: false
                , success: function (layero, index) {
                    layer.style(index, {
                        marginLeft: -15
                    });
                }
            });
        });
        siteDir.find('li').on('click', function () {
            var othis = $(this);
            othis.find('a').addClass('layui-this');
            othis.siblings().find('a').removeClass('layui-this');
        });
    }

    //在textarea焦点处插入字符
    var focusInsert = function (str) {
        var start = this.selectionStart
            , end = this.selectionEnd
            , offset = start + str.length

        this.value = this.value.substring(0, start) + str + this.value.substring(end);
        this.setSelectionRange(offset, offset);
    };

    //演示页面
    $('body').on('keydown', '#LAY_editor, .site-demo-text', function (e) {
        var key = e.keyCode;
        if (key === 9 && window.getSelection) {
            e.preventDefault();
            focusInsert.call(this, '  ');
        }
    });

    var editor = $('#LAY_editor')
        , iframeElem = $('#LAY_demo')
        , demoForm = $('#LAY_demoForm')[0]
        , demoCodes = $('#LAY_demoCodes')[0]
        , runCodes = function () {
            if (!iframeElem[0]) return;
            var html = editor.val();

            html = html.replace(/=/gi, "layequalsign");
            html = html.replace(/script/gi, "layscrlayipttag");
            demoCodes.value = html.length > 100 * 1000 ? '<h1>卧槽，你的代码过长</h1>' : html;

            demoForm.action = '/api/runHtml/';
            demoForm.submit();

        };
    $('#LAY_demo_run').on('click', runCodes), runCodes();

    //让导航在最佳位置
    var setScrollTop = function (thisItem, elemScroll) {
        if (thisItem[0]) {
            var itemTop = thisItem.offset().top
                , winHeight = $(window).height();
            if (itemTop > winHeight - 120) {
                elemScroll.animate({ 'scrollTop': itemTop / 2 }, 200)
            }
        }
    }
    setScrollTop($('.site-demo-nav').find('dd.layui-this'), $('.layui-side-scroll').eq(0));
    setScrollTop($('.site-demo-table-nav').find('li.layui-this'), $('.layui-side-scroll').eq(1));



    //查看代码
    $(function () {
        var DemoCode = $('#LAY_democode');
        DemoCode.val([
            DemoCode.val()
            , '<body>'
            , global.preview
            , '\n<script src="//res.layui.com/layui/dist/layui.js" charset="utf-8"></script>'
            , '\n<!-- 注意：如果你直接复制所有代码到本地，上述js路径需要改成你本地的 -->'
            , $('#LAY_democodejs').html()
            , '\n</body>\n</html>'
        ].join(''));
    });

    //点击查看代码选项
    element.on('tab(demoTitle)', function (obj) {
        if (obj.index === 1) {
            if (device.ie && device.ie < 9) {
                layer.alert('强烈不推荐你通过ie8/9 查看代码！因为，所有的标签都会被格式成大写，且没有换行符，影响阅读');
            }
        }
    })


    //手机设备的简单适配
    var treeMobile = $('.site-tree-mobile')
        , shadeMobile = $('.site-mobile-shade')

    treeMobile.on('click', function () {
        $('body').addClass('site-mobile');
    });

    shadeMobile.on('click', function () {
        $('body').removeClass('site-mobile');
    });



    //愚人节
    ; !function () {
        if (home.data('date') === '4-1') {

            if (local['20180401']) return;

            home.addClass('site-out-up');
            setTimeout(function () {
                layer.photos({
                    photos: {
                        "data": [{
                            "src": "//cdn.layui.com/upload/2018_4/168_1522515820513_397.png",
                        }]
                    }
                    , anim: 2
                    , shade: 1
                    , move: false
                    , end: function () {
                        layer.msg('愚公，快醒醒！', {
                            shade: 1
                        }, function () {
                            layui.data('layui', {
                                key: '20180401'
                                , value: true
                            });
                        });
                    }
                    , success: function (layero, index) {
                        home.removeClass('site-out-up');

                        layero.find('#layui-layer-photos').on('click', function () {
                            layer.close(layero.attr('times'));
                        }).find('.layui-layer-imgsee').remove();
                    }
                });
            }, 1000 * 3);
        }
    }();

    
    exports('global', {});
});