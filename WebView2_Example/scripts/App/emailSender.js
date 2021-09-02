function emailSendingModule(mailTemplate) {

    this.isValidCorrespondenceBody = function (body) {
        if (!body)
            body = this.getText();
        var parser = new DOMParser();
        var bodyDOM = parser.parseFromString(body, 'text/html');
        var mandatoryElements = bodyDOM.getElementsByClassName('mandatory');

        for (const el of mandatoryElements) {
            if (el.innerText.includes('{')
                || el.innerText.includes('}')
                || !el.innerText.trim()) {
                return false;
            }
        }

        return true;
    }

    function decodeHtml(encodedText) {
        let myText = document.createElement('textarea');
        myText.innerHTML = encodedText;
        return (myText.value);
    }

    function addStyle(styleString) {
        var sheet = document.createElement('style')
        sheet.innerHTML = styleString;
        document.body.appendChild(sheet);
        //var sheet = window.document.styleSheets[0];
        //sheet.addRule(styleString);
    }

    this.getText = function () {
        return decodeHtml(tinymce.activeEditor.getContent());
    }

    this.emailTemplateEditor = function (body) {

        decodedBody = decodeHtml(body);
        bodyWithReplacedPlaceholders = removePlaceholdersTokens(decodedBody);

        let iframeBody = `
            <div class="tiny-email-editor-container">
                <div class="non-editable">
                    ${bodyWithReplacedPlaceholders}
                </div>
            </div>`;

        let textArea = document.getElementById('tinyTextArea');
        textArea.innerText = iframeBody;

        tinymce.init({
            selector: '#tinyTextArea',
            plugins: ['advlist autolink lists link image charmap',
                'searchreplace fullscreen autoresize noneditable media'],
            menubar: false,
            table_grid: false,
            visual: false,
            table_resize_bars: false,
            table_advtab: false,
            table_row_advtab: false,
            table_cell_advtab: false,
            object_resizing: false,
            elementpath: false,
            statusbar: false,
            branding: false,
            noneditable_noneditable_class: 'non-editable',
            noneditable_editable_class: 'editable',
            toolbar: 'undo redo | bold italic underline | fontselect fontsizeselect formatselect | alignleft aligncenter alignright | numlist bullist | forecolor backcolor | link anchor | media',
            setup: function (ed) {
                ed.on('init', () => {
                    setLastEditorBodyToNonEditable();
                    traverseDomTree(tinyMCE.editors[tinyMCE.editors.length - 1].dom.doc.body, transformTemplateForEditor);
                    addStyle(`
                    .tox-pop__dialog {
                        display: none !important;
                    }`)
                });
            }
        });
    }

    function setLastEditorBodyToNonEditable() {
        tinyMCE.editors[tinyMCE.editors.length - 1].dom.doc.body.setAttribute("contenteditable", "false");
    }

    function removePlaceholdersTokens(body) {
        const plceholdersTokesRegex = /##m%%|%%/g;
        var bodyWithReplacedPlaceholders = body.replace(plceholdersTokesRegex, '');

        return bodyWithReplacedPlaceholders;
    }

    var me = this;
    me.template = mailTemplate;

    function traverseDomTree(rootElement, func) {
        for (let i = 0; i < rootElement.children.length; i++) {
            func(rootElement.children[i]);
            traverseDomTree(rootElement.children[i], func)
        };
    }

    function onFocusEditable(event) {
        var el = event.currentTarget;
        if (!el.originalValue) {
            el.originalValue = el.innerHTML;
        }
        setTimeout(() => {
            el.innerHTML = emptyValue;
        }, 50)
    }

    function onBlurEditable(event) {
        var el = event.currentTarget;
        if (el.innerHTML != emptyValue) {
            el.removeEventListener('blur', onBlurEditable);
            el.removeEventListener('focus', onFocusEditable)
            return;
        }
    }

    function transformTemplateForEditor(currentElement) {
        var emptyValue = '&nbsp;';

        if (!currentElement.classList.contains('editable')) {
            currentElement.setAttribute('contenteditable', 'false');
        } else {
            currentElement.style.backgroundColor = "yellow";
            currentElement.addEventListener('focus', onFocusEditable);
            currentElement.addEventListener('blur', onBlurEditable);
        }
    }
}