console.log('[DEBUG] login.js carregado');
// =====================================================
// FUNCIONALIDADE "SALVAR CREDENCIAIS" COM LOCALSTORAGE
// =====================================================

// Chaves para localStorage
const CREDENCIAIS_EMAIL_KEY = 'dorowcamp_credenciais_email';
const CREDENCIAIS_SENHA_KEY = 'dorowcamp_credenciais_senha';
const CREDENCIAIS_SALVAR_KEY = 'dorowcamp_credenciais_salvar';

// Função para salvar credenciais no localStorage
function salvarCredenciaisLocalStorage(email, senha) {
    try {
        // Verificar se localStorage está disponível
        if (typeof(Storage) === "undefined") {
            console.error('localStorage não está disponível');
            return false;
        }
        
        // Testar se podemos escrever no localStorage
        const testKey = 'test_write_' + Date.now();
        localStorage.setItem(testKey, 'test');
        const testValue = localStorage.getItem(testKey);
        localStorage.removeItem(testKey);
        
        if (testValue !== 'test') {
            console.error('localStorage não está funcionando corretamente');
            return false;
        }
        
        // Salvar credenciais
        localStorage.setItem(CREDENCIAIS_EMAIL_KEY, email);
        localStorage.setItem(CREDENCIAIS_SENHA_KEY, senha);
        localStorage.setItem(CREDENCIAIS_SALVAR_KEY, 'true');
        
        console.log('Credenciais salvas no localStorage com sucesso');
        return true;
    } catch (error) {
        console.error('Erro ao salvar credenciais:', error);
        console.error('Tipo de erro:', error.name);
        console.error('Mensagem:', error.message);
        return false;
    }
}

// Função para carregar credenciais do localStorage
function carregarCredenciaisLocalStorage() {
    try {
        const email = localStorage.getItem(CREDENCIAIS_EMAIL_KEY);
        const senha = localStorage.getItem(CREDENCIAIS_SENHA_KEY);
        const salvar = localStorage.getItem(CREDENCIAIS_SALVAR_KEY);
        
        return {
            email: email || '',
            senha: senha || '',
            salvar: salvar === 'true'
        };
    } catch (error) {
        console.error('Erro ao carregar credenciais:', error);
        return { email: '', senha: '', salvar: false };
    }
}

// Função para remover credenciais do localStorage
function removerCredenciaisLocalStorage() {
    try {
        localStorage.removeItem(CREDENCIAIS_EMAIL_KEY);
        localStorage.removeItem(CREDENCIAIS_SENHA_KEY);
        localStorage.removeItem(CREDENCIAIS_SALVAR_KEY);
        console.log('Credenciais removidas do localStorage');
        return true;
    } catch (error) {
        console.error('Erro ao remover credenciais:', error);
        return false;
    }
}

// Função para mostrar alerta Bootstrap
function mostrarAlerta(tipo, mensagem) {
    const alertArea = document.getElementById('alertArea');
    if (!alertArea) return;
    
    // Remover alertas anteriores
    alertArea.innerHTML = '';
    
    // Criar novo alerta
    const alerta = document.createElement('div');
    alerta.className = `alert alert-${tipo}`;
    
    const icone = tipo === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle';
    alerta.innerHTML = `
        <i class="bi ${icone}"></i>
        <span>${mensagem}</span>
    `;
    
    alertArea.appendChild(alerta);
    
    // Remover alerta após 5 segundos usando setTimeout()
    setTimeout(() => {
        if (alerta.parentNode) {
            alerta.remove();
        }
    }, 5000);
}

// Função para lidar com mudança no checkbox
function handleCheckboxChange() {
    console.log('handleCheckboxChange chamada');
    
    const checkbox = document.getElementById('salvarCredenciais');
    const emailInput = document.getElementById('login_user_x');
    const senhaInput = document.getElementById('i_x2');
    
    if (!checkbox) {
        console.error('Checkbox não encontrado');
        return;
    }
    if (!emailInput) {
        console.error('Campo e-mail não encontrado');
        return;
    }
    if (!senhaInput) {
        console.error('Campo senha não encontrado');
        return;
    }
    
    console.log('Estado do checkbox:', checkbox.checked);
    
    // Verificar se localStorage está disponível
    if (typeof(Storage) === "undefined") {
        console.error('localStorage não está disponível');
        mostrarAlerta('danger', 'localStorage não está disponível neste navegador.');
        return;
    }
    
    if (checkbox.checked) {
        // Checkbox marcado - salvar credenciais
        const email = emailInput.value.trim();
        const senha = btoa(senhaInput.value);
        
        console.log('Tentando salvar credenciais:', { email: email ? 'preenchido' : 'vazio', senha: senha ? 'preenchido' : 'vazio' });
        
        if (email && senha) {
            const sucesso = salvarCredenciaisLocalStorage(email, senha);
            if (sucesso) {
                mostrarAlerta('success', 'As credenciais foram salvas com sucesso.');
            } else {
                mostrarAlerta('danger', 'Erro ao salvar credenciais.');
            }
        } else {
            mostrarAlerta('warning', 'Digite e-mail e senha para salvar as credenciais.');
        }
    } else {
        // Checkbox desmarcado - remover credenciais
        console.log('Removendo credenciais');
        const sucesso = removerCredenciaisLocalStorage();
        if (sucesso) {
            mostrarAlerta('warning', 'As credenciais não serão salvas.');
        } else {
            mostrarAlerta('danger', 'Erro ao remover credenciais.');
        }
    }
}

// Função para inicializar credenciais salvas
function inicializarCredenciaisSalvas() {
    console.log('Inicializando credenciais salvas...');
    
    // Verificar se localStorage está disponível
    if (typeof(Storage) === "undefined") {
        console.error('localStorage não está disponível para inicialização');
        return;
    }
    
    const credenciais = carregarCredenciaisLocalStorage();
    const emailInput = document.getElementById('login_user_x');
    const senhaInput = document.getElementById('i_x2');
    const checkbox = document.getElementById('salvarCredenciais');
    
    if (!emailInput) {
        console.error('Campo e-mail não encontrado para inicialização');
        return;
    }
    if (!senhaInput) {
        console.error('Campo senha não encontrado para inicialização');
        return;
    }
    if (!checkbox) {
        console.error('Checkbox não encontrado para inicialização');
        return;
    }
    
    console.log('Credenciais encontradas:', { 
        salvar: credenciais.salvar, 
        temEmail: !!credenciais.email, 
        temSenha: !!credenciais.senha 
    });
    
    if (credenciais.salvar && credenciais.email && credenciais.senha) {
        // Preencher campos
        emailInput.value = credenciais.email;
        senhaInput.value = credenciais.senha;
        
        // Atualizar campo hidden com a senha ofuscada
        const senhaHiddenInput = document.getElementById('i_x2_encoded');
        if (senhaHiddenInput) {
            senhaHiddenInput.value = btoa(credenciais.senha);
        }
        
        // Marcar checkbox
        checkbox.checked = true;
        
        console.log('✅ Credenciais carregadas e campos preenchidos com sucesso');
    } else {
        console.log('ℹ️ Nenhuma credencial salva encontrada ou dados incompletos');
    }
}

// Funções para o modal "Esqueci minha senha"
function esqueciSenha() {
    const modal = document.getElementById('forgotPasswordModal');
    modal.classList.add('show');
    document.getElementById('resetEmail').focus();
}

function fecharModal() {
    const modal = document.getElementById('forgotPasswordModal');
    modal.classList.remove('show');
    // Limpar formulário
    const form = document.getElementById('forgotPasswordForm');
    if (form) {
        form.reset();
    }
    const emailInput = document.getElementById('resetEmail');
    if (emailInput) {
        emailInput.classList.remove('is-invalid', 'is-valid');
    }
    // Remover loading se estiver ativo
    const sendResetBtn = document.getElementById('sendResetBtn');
    if (sendResetBtn) {
        sendResetBtn.classList.remove('loading');
        sendResetBtn.disabled = false;
    }
}

function criarConta() {
    window.location.href = '/Login/CriarConta';
}

// Função para mostrar mensagens
function showMessage(type, message, duration = 5000) {
    // Remover mensagens existentes
    const existingMessages = document.querySelectorAll('.message-container');
    existingMessages.forEach(msg => msg.remove());

    // Criar nova mensagem
    const messageDiv = document.createElement('div');
    messageDiv.className = `message-container message-${type}`;
    messageDiv.innerHTML = `
        <div class="message-content">
            <div class="message-icon">
                <i class="bi ${getMessageIcon(type)}"></i>
            </div>
            <div class="message-text">${message}</div>
            <button class="message-close" onclick="this.parentElement.parentElement.remove()">
                <i class="bi bi-x"></i>
            </button>
        </div>
    `;

    // Adicionar ao body
    document.body.appendChild(messageDiv);

    // Mostrar com animação
    setTimeout(() => {
        messageDiv.classList.add('message-show');
    }, 100);

    // Remover automaticamente
    if (duration > 0) {
        setTimeout(() => {
            messageDiv.classList.remove('message-show');
            messageDiv.classList.add('message-hide');
            setTimeout(() => {
                if (messageDiv.parentElement) {
                    messageDiv.remove();
                }
            }, 300);
        }, duration);
    }
}

function getMessageIcon(type) {
    switch (type) {
        case 'success': return 'bi-check-circle';
        case 'error': return 'bi-exclamation-circle';
        case 'warning': return 'bi-exclamation-triangle';
        case 'info': return 'bi-info-circle';
        default: return 'bi-info-circle';
    }
}

// Event listeners quando o DOM estiver carregado
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM carregado - Iniciando configuração do checkbox...');

    // Aguardar um pouco para garantir que todos os elementos estejam carregados
    setTimeout(() => {
        // Inicializar credenciais salvas
        inicializarCredenciaisSalvas();
        
        // Adicionar event listener para o checkbox
        const checkbox = document.getElementById('salvarCredenciais');
        if (checkbox) {
            console.log('Checkbox encontrado, adicionando event listener');
            // Remover event listener anterior se existir
            checkbox.removeEventListener('change', handleCheckboxChange);
            // Adicionar novo event listener
            checkbox.addEventListener('change', handleCheckboxChange);
            
            // Adicionar também event listener para clique (para compatibilidade com Chrome)
            checkbox.addEventListener('click', function(e) {
                console.log('Clique no checkbox detectado');
                // Pequeno delay para garantir que o estado seja atualizado
                setTimeout(() => {
                    handleCheckboxChange();
                }, 10);
            });
        } else {
            console.error('Checkbox não encontrado!');
        }
        
                // Adicionar event listeners para os campos de e-mail e senha
        const emailInput = document.getElementById('login_user_x');
        const senhaInput = document.getElementById('i_x2');
        
        // Event listeners para salvar credenciais quando campos mudarem
        if (emailInput) {
            emailInput.addEventListener('input', function() {
                const checkbox = document.getElementById('salvarCredenciais');
                if (checkbox && checkbox.checked) {
                    const email = this.value.trim();
                    const senha = senhaInput ? btoa(senhaInput.value) : '';
                    if (email && senha) {
                        salvarCredenciaisLocalStorage(email, senha);
                    }
                }
            });
        }
        
        if (senhaInput) {
            senhaInput.addEventListener('input', function() {
                const checkbox = document.getElementById('salvarCredenciais');
                if (checkbox && checkbox.checked) {
                    const email = emailInput ? emailInput.value.trim() : '';
                    const senha = btoa(this.value);
                    if (email && senha) {
                        salvarCredenciaisLocalStorage(email, senha);
                    }
                }
            });
        }
        
        console.log('Configuração do checkbox concluída');
    }, 100); // Delay de 100ms para garantir carregamento completo
    
    // Fechar modal ao clicar fora dele
    const modal = document.getElementById('forgotPasswordModal');
    if (modal) {
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                fecharModal();
            }
        });
    }

    // Fechar modal com ESC
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            fecharModal();
        }
    });

    // Formulário de redefinição de senha
    const forgotPasswordForm = document.getElementById('forgotPasswordForm');
    if (forgotPasswordForm) {
        forgotPasswordForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const emailInput = document.getElementById('resetEmail');
            const email = emailInput.value.trim();
            const sendResetBtn = document.getElementById('sendResetBtn');
            
            // Validação básica
            if (!email || !isValidEmail(email)) {
                emailInput.classList.add('is-invalid');
                emailInput.classList.remove('is-valid');
                showMessage('error', 'Por favor, informe um e-mail válido.', 5000);
                return;
            }
            
            emailInput.classList.remove('is-invalid');
            emailInput.classList.add('is-valid');
            
            // Mostrar loading
            sendResetBtn.classList.add('loading');
            sendResetBtn.disabled = true;
            
            try {
                const response = await fetch('/Login/ForgotPassword', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ Email: email })
                });
                
                const result = await response.json();
                
                if (result.success) {
                    showMessage('success', result.message, 8000);
                    // Fechar modal após sucesso
                    setTimeout(() => {
                        fecharModal();
                    }, 2000);
                } else {
                    showMessage('error', result.message, 8000);
                }
            } catch (error) {
                showMessage('error', 'Erro ao processar solicitação. Tente novamente.', 5000);
            } finally {
                // Remover loading
                sendResetBtn.classList.remove('loading');
                sendResetBtn.disabled = false;
            }
        });
    }

    // Validação de e-mail em tempo real
    const resetEmailInput = document.getElementById('resetEmail');
    if (resetEmailInput) {
        resetEmailInput.addEventListener('input', function() {
            const email = this.value.trim();
            if (email.length > 0) {
                if (isValidEmail(email)) {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                } else {
                    this.classList.add('is-invalid');
                    this.classList.remove('is-valid');
                }
            } else {
                this.classList.remove('is-invalid', 'is-valid');
            }
        });
    }

    // Formulário de login
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            // Prevenir o submit padrão para aplicar ofuscação
            e.preventDefault();
            console.log('[DEBUG] HTML do formulário:', loginForm.innerHTML);
            console.log('[DEBUG] Submit do login iniciado');
            console.log('[DEBUG] window.grecaptcha:', window.grecaptcha);
            console.log('[DEBUG] window.recaptchaSiteKey:', window.recaptchaSiteKey);
            console.log('[DEBUG] Campo hidden:', document.getElementById('g_recaptcha_response'));
            const emailInput = document.getElementById('login_user_x');
            const senhaInput = document.getElementById('i_x2');
            const loginBtn = document.getElementById('loginBtn');
            
            // Validações básicas
            let isValid = true;
            
            if (!emailInput.value.trim()) {
                emailInput.classList.add('is-invalid');
                isValid = false;
            } else {
                emailInput.classList.remove('is-invalid');
            }
            
            if (!senhaInput.value.trim()) {
                senhaInput.classList.add('is-invalid');
                isValid = false;
            } else {
                senhaInput.classList.remove('is-invalid');
            }
            
            if (!isValid) {
                showMessage('error', 'Por favor, preencha todos os campos obrigatórios.', 5000);
                return;
            }
            
            // Ofuscar a senha antes do envio para evitar que apareça em texto puro no payload
            if (senhaInput && senhaInput.value) {
                const senhaOriginal = senhaInput.value;
                const senhaOfuscada = btoa(senhaInput.value);
                // Usar o campo hidden para enviar a senha ofuscada
                const senhaHiddenInput = document.getElementById('i_x2_encoded');
                if (senhaHiddenInput) {
                    senhaHiddenInput.value = senhaOfuscada;
                    console.log('[DEBUG] Senha ofuscada:', { original: senhaOriginal, ofuscada: senhaOfuscada });
                }
            }
            
            // Integração reCAPTCHA v3 condicional
            if (window.recaptchaEnabled) {
                if (window.grecaptcha && window.recaptchaSiteKey) {
                    grecaptcha.ready(function() {
                        grecaptcha.execute(window.recaptchaSiteKey, { action: 'login' }).then(function(token) {
                            var recaptchaInput = document.getElementById('g_recaptcha_response');
                            if (recaptchaInput) {
                                console.log('[DEBUG] Campo hidden encontrado, preenchendo token e submetendo formulário.');
                                recaptchaInput.value = token;
                                // Garantir que a senha esteja ofuscada antes do submit final
                                const senhaInputFinal = document.getElementById('i_x2');
                                const senhaHiddenInputFinal = document.getElementById('i_x2_encoded');
                                if (senhaInputFinal && senhaInputFinal.value && senhaHiddenInputFinal) {
                                    // Ofuscar a senha no campo hidden
                                    senhaHiddenInputFinal.value = btoa(senhaInputFinal.value);
                                }
                                // Mostrar loading
                                loginBtn.classList.add('loading');
                                loginBtn.disabled = true;
                                // Submeter formulário
                                loginForm.submit();
                            } else {
                                console.error('[reCAPTCHA] Campo hidden g_recaptcha_response não encontrado!');
                                return; // Impede qualquer ação se o campo não existe
                            }
                        });
                    });
                } else {
                    // Mostrar loading normalmente se reCAPTCHA não carregou
                    loginBtn.classList.add('loading');
                    loginBtn.disabled = true;
                    // Submeter formulário sem reCAPTCHA
                    loginForm.submit();
                }
            } else {
                // Se reCAPTCHA desativado, submeter normalmente
                // Mostrar loading
                loginBtn.classList.add('loading');
                loginBtn.disabled = true;
                // Submeter formulário
                loginForm.submit();
            }
        });
    }
});

// Função para validar e-mail
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

 