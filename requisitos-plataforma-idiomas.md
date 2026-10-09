# Levantamento de requisitos — Plataforma de idiomas com vídeos

Versão: 1.0 · Data: 08/10/2026 · Status: proposta inicial para orientar desenvolvimento.

## 1. Objetivo

Permitir que uma pessoa aprenda idiomas assistindo a vídeos próprios, acompanhando legendas no idioma original, consultando traduções de palavras em seu contexto e criando cards para estudar no Anki.

O fluxo principal é: **enviar vídeo → processar → assistir com legendas → clicar em palavra → consultar tradução contextual → revisar card → enviar ao Anki ou exportar arquivo**.

Este documento registra o comportamento desejado. Não afirma que os recursos já existem na aplicação. A única evidência técnica recebida foi o `docker-compose.yml`; o código ainda precisa ser analisado.

## 2. Escopo e prioridades

| Prioridade | Significado |
| --- | --- |
| P0 | Necessário para completar o fluxo principal do MVP. |
| P1 | Incremento após validar o fluxo principal. |
| P2 | Evolução futura. |

| Entrega | Escopo |
| --- | --- |
| MVP — P0 | Upload, processamento assíncrono, identificação do idioma, transcrição original com tempos, player com legenda clicável, tradução da frase e da palavra, criação/revisão de cards, lista de cards e exportação de texto para Anki. |
| Integração condicionada — P1 | Envio direto para Anki, após prova técnica no ambiente real do usuário. Pode entrar no MVP se a viabilidade for demonstrada sem atrasar o fluxo principal. |
| Evolução — P2 | Entrada por link do YouTube e reutilização do fluxo de aprendizado. |
| Fora do MVP | Revisão espaçada própria, compartilhamento público, pagamentos, cursos, gamificação, aplicativos móveis nativos, cards com áudio/imagem e geração de arquivos `.apkg`. |

A ordem acima é uma recomendação de entrega. A preferência do produto continua sendo enviar diretamente ao Anki quando possível e oferecer exportação por arquivo quando indisponível.

## 3. Usuário e jornada

**Usuário principal:** estudante que possui um vídeo em um idioma que quer aprender e escolhe outro idioma para compreender o conteúdo.

1. Acessa sua área de vídeos e escolhe um arquivo.
2. Envia o vídeo e acompanha a etapa de processamento.
3. Confere o idioma identificado; corrige quando necessário.
4. Abre o vídeo pronto, escolhe o idioma de tradução e acompanha a legenda original.
5. Clica em uma palavra; o player pausa e aparece um painel de tradução.
6. Vê a palavra original, seu significado naquele contexto, a frase original completa e sua tradução.
7. Revisa o card e usa “Enviar ao Anki” ou “Adicionar à lista”.
8. Continua assistindo e, quando quiser, exporta os cards da lista.

**Proposta de UX:** pausar ao abrir a consulta e retomar somente por ação do usuário. Essa escolha evita perder o trecho enquanto lê a tradução.

## 4. Requisitos funcionais

### 4.1. Acesso e biblioteca

| ID | Prioridade | Requisito | Critério de aceitação |
| --- | --- | --- | --- |
| RF-01 | P0 | Identificar o usuário e restringir seus vídeos, traduções e cards. Reaproveitar autenticação existente se confirmada no código. | Um usuário não consegue consultar, reproduzir, traduzir ou alterar recursos de outro por URL ou ID direto. |
| RF-02 | P0 | Listar os vídeos com título, idioma identificado, data e estado. | Um upload aceito aparece na lista, e o estado pode ser atualizado sem manter a tela de upload aberta. |
| RF-03 | P0 | Excluir um vídeo próprio e iniciar a limpeza de seus arquivos e dados derivados. | O vídeo deixa de ser acessível; cards já salvos preservam seu texto e passam a indicar fonte indisponível. |

### 4.2. Upload e processamento

| ID | Prioridade | Requisito | Critério de aceitação |
| --- | --- | --- | --- |
| RF-04 | P0 | Receber upload com formatos, tamanho e duração máximos configuráveis. Validar também o conteúdo da mídia. | Arquivo inválido ou acima do limite é recusado com motivo claro; upload interrompido não é marcado como concluído. |
| RF-05 | P0 | Mostrar progresso de envio separado do processamento. | A interface distingue “enviando arquivo” de “processando vídeo”, sem inventar percentual de processamento. |
| RF-06 | P0 | Persistir o arquivo e vincular seu registro ao usuário. | Após upload confirmado, arquivo e registro podem ser recuperados, inclusive depois de reiniciar a aplicação. |
| RF-07 | P0 | Processar em segundo plano, sem bloquear a requisição HTTP durante a transcrição. | A requisição termina após aceitar o trabalho; o usuário acompanha o resultado posteriormente. |
| RF-08 | P0 | Detectar o idioma predominante e permitir confirmação/correção. | A tela mostra o idioma identificado ou pede escolha quando a detecção é inconclusiva; uma correção permite gerar nova transcrição. |
| RF-09 | P0 | Gerar transcrição no idioma falado, com segmentos e tempos relativos ao vídeo. | Texto e intervalos de tempo ficam disponíveis para reprodução sincronizada; a transcrição original não é substituída pela tradução. |
| RF-10 | P0 | Expor etapa atual, conclusão, falha e ação de tentar novamente quando cabível. | Falha no provedor não deixa o vídeo indefinidamente em processamento; a interface apresenta uma mensagem compreensível. |
| RF-11 | P0 | Tratar vídeos sem áudio, áudio sem fala e formatos não decodificáveis. | O usuário recebe o diagnóstico adequado e não vê uma transcrição inventada ou um vídeo “pronto” com resultado inválido. |

### 4.3. Reprodução e consulta contextual

| ID | Prioridade | Requisito | Critério de aceitação |
| --- | --- | --- | --- |
| RF-12 | P0 | Reproduzir, pausar e avançar/retroceder o vídeo, com ajuste de velocidade. | O player preserva a sincronização depois de uma busca temporal, pausa ou mudança de velocidade. |
| RF-13 | P0 | Exibir legenda original sincronizada e palavras selecionáveis. | A legenda acompanha o tempo atual e aceita clique/toque e seleção por teclado. |
| RF-14 | P0 | Permitir escolher o idioma de destino entre os efetivamente suportados. | O idioma escolhido aparece no painel e nos cards criados; resultados anteriores não são apresentados como se fossem do novo idioma. |
| RF-15 | P0 | Identificar a ocorrência exata da palavra clicada e recuperar sua frase completa. | Duas ocorrências da mesma palavra podem produzir consultas diferentes quando o contexto muda. |
| RF-16 | P0 | Mostrar tradução da frase inteira e significado contextual da palavra selecionada. | O painel contém os quatro elementos: palavra original, significado contextual, frase original e frase traduzida. |
| RF-17 | P0 | Reconhecer quando o significado pertence a uma expressão de várias palavras. | Em “flesh out your plan”, o clique em “flesh” pode exibir “flesh out — detalhar/elaborar”, mantendo a palavra originalmente selecionada. |
| RF-18 | P0 | Exibir carregamento, falha e nova tentativa para a consulta. | Cliques sucessivos não permitem que uma resposta antiga substitua a consulta mais recente. |
| RF-19 | P1 | Exibir a transcrição completa em painel e permitir saltar para um trecho. | Ao selecionar um segmento, o player busca seu tempo inicial e atualiza a legenda. |
| RF-20 | P1 | Permitir selecionar manualmente uma expressão e ampliar o contexto. | O usuário pode corrigir uma seleção inadequada sem modificar a transcrição inteira. |

**Definição de contexto:** a unidade de legenda pode ser apenas parte de uma frase. O sistema deve manter a relação entre palavras, segmentos e frases, reconstruindo a frase que contém a ocorrência clicada. Quando a frase for ambígua ou não houver pontuação confiável, utilizar uma janela limitada de segmentos adjacentes e informar que o contexto é aproximado. Os limites dessa janela devem ser configuráveis e validados com vídeos reais.

O significado da palavra não exige correspondência literal com uma palavra na frase traduzida. Expressões, flexões e traduções naturais podem alterar essa relação. Explicação curta é útil quando necessária para entender esse caso; explicações gramaticais extensas não são obrigatórias no MVP.

### 4.4. Cards e lista de exportação

| ID | Prioridade | Requisito | Critério de aceitação |
| --- | --- | --- | --- |
| RF-21 | P0 | Criar uma prévia de card a partir da consulta concluída. | A prévia preserva palavra/expressão, frase, traduções, idiomas, fonte e tempo do trecho. |
| RF-22 | P0 | Permitir revisar e editar frente/verso antes de salvar ou enviar. | Alterações manuais aparecem no item salvo e no arquivo exportado. |
| RF-23 | P0 | Adicionar cards a uma lista de preparação por usuário. | A lista mostra contador, cards e ações de editar/remover; erros de envio ao Anki preservam o card. |
| RF-24 | P0 | Oferecer exportação a partir de um card e permitir exportar vários. | A ação fica disponível quando a lista não está vazia; não há quantidade mínima artificial. |
| RF-25 | P0 | Sinalizar inclusão repetida do mesmo card, permitindo decisão do usuário. | Cliques repetidos não criam duplicatas acidentais; a mesma palavra em outra frase continua permitida. |
| RF-26 | P0 | Gerar `.txt` com campos separados por tabulação em UTF-8. | Arquivo pode ser importado no Anki com frente e verso corretos, preservando acentos e caracteres não latinos. |
| RF-27 | P1 | Oferecer CSV como formato adicional. | Vírgulas, aspas, quebras de linha e separadores são tratados sem deslocar os campos. |
| RF-28 | P0 | Orientar o usuário sobre importação e permitir baixar novamente. | Exportar não apaga a lista automaticamente e não marca os cards como importados no Anki. |

**Lista momentânea:** a lista representa cards aguardando exportação, sem implementar estudo espaçado. Recomenda-se persistir a lista por usuário para evitar perda ao recarregar ou fechar a página. Essa persistência é uma proposta, não uma exigência expressa do pedido original.

### 4.5. Integração direta com Anki

| ID | Prioridade | Requisito | Critério de aceitação |
| --- | --- | --- | --- |
| RF-29 | P1 | Validar uma integração local, tendo AnkiConnect como candidato. | Uma prova técnica demonstra envio a uma instalação real do Anki e documenta pré-requisitos e ambientes suportados. |
| RF-30 | P1 | Permitir escolher baralho, tipo de nota e mapeamento de campos compatíveis. | O card chega ao baralho escolhido com conteúdo correto; configuração incompatível produz erro útil. |
| RF-31 | P1 | Confirmar sucesso somente após resposta válida da integração. | A aplicação registra o identificador devolvido pela integração e distingue sucesso, falha e resultado incerto. |
| RF-32 | P1 | Oferecer “Adicionar à lista” se a integração estiver indisponível. | Anki fechado, complemento ausente ou acesso bloqueado não impede salvar e exportar o card. |

Não presumir que um backend hospedado consegue acessar o Anki no computador do usuário. A prova técnica deve definir onde ocorre a conexão local — navegador, extensão ou componente local — e verificar restrições do navegador e configuração de origem. Não ampliar a exposição de rede do Anki como requisito do produto.

### 4.6. YouTube — evolução futura

| ID | Prioridade | Requisito | Critério de aceitação |
| --- | --- | --- | --- |
| RF-33 | P2 | Aceitar link e identificar o vídeo sem upload manual. | Link inválido é recusado; conteúdo indisponível gera estado e mensagem claros. |
| RF-34 | P2 | Reproduzir com player incorporado e sincronizar a camada própria de legendas. | Pausa, busca temporal e tempo atual alimentam a mesma experiência de aprendizado. |
| RF-35 | P2 | Obter transcrição e tempos por uma fonte tecnicamente viável e autorizada. | A disponibilidade é verificada antes de prometer processamento; a aplicação informa quando não consegue obter os dados necessários. |
| RF-36 | P2 | Reutilizar tradução contextual, cards e exportação. | A origem “YouTube” não exige duplicar a lógica de aprendizado. |

Incorporar um player não resolve, por si só, a obtenção de áudio ou transcrição. A estratégia de aquisição deve ser investigada em uma entrega própria; não é dependência do MVP de uploads.

## 5. Regras de negócio

| ID | Regra |
| --- | --- |
| RN-01 | Idioma de origem e idioma de destino são independentes. A legenda original permanece no idioma de origem. |
| RN-02 | MVP: um idioma predominante por vídeo. Vídeos com alternância de idiomas devem informar essa limitação; detecção por trecho é evolução. |
| RN-03 | A consulta utiliza a ocorrência selecionada e seu contexto, nunca somente a palavra isolada. |
| RN-04 | Cards guardam uma cópia do conteúdo revisado. Reprocessar ou excluir o vídeo não altera automaticamente cards existentes. |
| RN-05 | Enviar um card exige ação do usuário. A tradução não dispara envio automático. |
| RN-06 | Gerar arquivo significa “exportado”, não “importado no Anki”. Reexportação é permitida. |
| RN-07 | Identidade de duplicata inclui usuário, vídeo, versão da transcrição, ocorrência/intervalo e idioma de destino. Alterações deliberadas podem gerar novo card. |
| RN-08 | Nova versão de transcrição invalida consultas em cache ligadas à versão anterior, preservando cards já salvos. |
| RN-09 | No MVP, cards são textuais. Áudio, imagem ou recorte de vídeo exigem outra entrega. |
| RN-10 | Uma resposta incerta do Anki não deve ser reenviada cegamente; verificar possível criação anterior ou orientar o usuário. |

## 6. Modelo inicial do card e contrato de exportação

Modelo proposto: **frente:** frase original com a palavra/expressão destacada; **verso:** tradução da frase, significado contextual da palavra/expressão e referência ao vídeo com o tempo.

Exemplo:

| Campo | Conteúdo |
| --- | --- |
| Palavra clicada | flesh |
| Expressão identificada | flesh out |
| Frase original | You need to flesh out your plan. |
| Tradução da frase | Você precisa detalhar seu plano. |
| Significado contextual | flesh out = detalhar, desenvolver melhor |
| Origem/destino | en → pt-BR |
| Fonte | vídeo de exemplo, 00:42 |

Contrato mínimo de exportação:

- Uma nota por registro, com duas colunas na ordem `Frente`, `Verso`.
- `.txt` tabulado e UTF-8 como formato obrigatório; CSV é alternativa futura.
- Nenhum cabeçalho comum deve virar card. Usar comentários/cabeçalhos reconhecidos pelo Anki ou fornecer arquivo sem cabeçalho comum.
- A exportação pode incluir HTML para destaque e quebras de linha; nesse caso, informar/definir importação com HTML e escapar o texto original antes de compor a marcação.
- Tratar separadores, aspas e quebras de linha com serialização apropriada, mantendo o mesmo número de campos em todas as notas.
- O arquivo simples deve ser importável em um tipo de nota com frente e verso. Um modelo com campos separados é evolução e exige instrução de configuração.
- A chave de duplicidade interna do sistema não determina sozinha o comportamento de duplicidade do Anki. Validar e explicar as opções de importação; não garantir deduplicação automática entre diferentes exportações no formato mínimo.

## 7. Requisitos não funcionais

| ID | Requisito | Evidência de atendimento |
| --- | --- | --- |
| RNF-01 | Isolamento por usuário em API, banco, objetos e URLs temporárias de mídia. | Tentativas de acesso cruzado recusadas; bucket privado e entrega de mídia autorizada. |
| RNF-02 | Processamento recuperável e idempotente. | Reinício do worker e evento repetido não criam múltiplas transcrições ativas nem corrompem o estado. |
| RNF-03 | Retentativas limitadas, com espera e tratamento de falhas definitivas. | Trabalho com erro persistente termina em falha visível e pode ser reexecutado de forma controlada. |
| RNF-04 | Chamadas externas com timeout, cancelamento quando aplicável e limites de concorrência. | Provedor lento não ocupa recursos indefinidamente; custo e concorrência podem ser limitados. |
| RNF-05 | Cache de tradução por origem, destino, contexto, ocorrência e versão. | Consultas equivalentes reaproveitam resultado; contextos/idiomas distintos não compartilham resposta incorreta. |
| RNF-06 | Transcrição e tradução protegidas contra execução de conteúdo. | Texto e nomes de arquivo são tratados como dados; marcações exibidas/exportadas são escapadas. |
| RNF-07 | Segredos separados do código e logs sem credenciais ou conteúdo completo dos vídeos. | Configuração de ambiente e registros permitem diagnóstico sem exposição desnecessária. |
| RNF-08 | Rastreabilidade por usuário, vídeo, trabalho, evento e tentativa. | É possível localizar a etapa que falhou, tempo gasto e causa técnica. |
| RNF-09 | Uso por mouse, toque e teclado, com foco e contraste legíveis. | A consulta e a lista de cards funcionam sem depender exclusivamente de clique de mouse. |
| RNF-10 | Reprodutibilidade do ambiente de desenvolvimento e prontidão das dependências. | Um clone com configuração documentada pode iniciar os serviços; aplicação aguarda/reconecta quando dependências ainda não estão prontas. |
| RNF-11 | Portabilidade de provedores de transcrição e tradução. | Contratos de aplicação não dependem diretamente de formatos exclusivos de um fornecedor em todas as camadas. |

**Metas iniciais propostas, sujeitas a medição:** atualização da legenda em até 300 ms após eventos do player; erro de alinhamento de até 500 ms em pelo menos 95% dos segmentos de um conjunto de vídeos anotado manualmente; tradução sem cache com p95 de até 5 s e com cache de até 500 ms, em ambiente e carga definidos. Atualização da interface e qualidade dos timestamps são medidas diferentes. Tempos de transcrição e limites de upload precisam ser definidos após escolher provedor e infraestrutura; não são SLAs assumidos.

## 8. Estados do vídeo e consistência

Estados propostos:

`Enviando → Aguardando processamento → Preparando áudio → Detectando idioma → Transcrevendo → Pronto`

Também existem `Falhou`, `Sem fala identificável` e `Excluído`. Falha durante envio não cria um trabalho pronto para consumir. Uma nova tentativa recebe identificação própria; a versão anterior pronta pode continuar disponível enquanto uma nova é gerada, conforme decisão de implementação.

Cada trabalho precisa registrar etapa, tentativas, versão, datas e erro técnico separado da mensagem ao usuário. O estado só chega a “Pronto” depois que a transcrição e seus tempos estiverem persistidos de forma consistente. Um resultado atrasado não deve restaurar vídeo excluído nem sobrescrever uma versão mais nova.

## 9. Dados conceituais

Modelo de domínio sugerido; não é uma prescrição de tabelas:

| Entidade | Dados centrais |
| --- | --- |
| Vídeo | ID, proprietário, título, origem, referência da mídia, duração, estado e idioma predominante. |
| Trabalho | ID, vídeo, tentativa, etapa, versão, datas, erro e identificação de correlação. |
| Transcrição | ID, vídeo, versão, idioma e origem/provedor. |
| Segmento | ID, transcrição, ordem, texto, início e fim em milissegundos. |
| Palavra/ocorrência | ID, segmento, posição no texto e referência à frase; tempo por palavra é opcional no MVP. |
| Frase/contexto | ID, versão, texto completo, segmentos associados e limites de seleção. |
| Consulta/tradução | Ocorrência, contexto, idiomas, frase traduzida, significado contextual e estado. |
| Card | ID, usuário, origem/versionamento, conteúdo revisado, idiomas, frente, verso e estado de envio. |
| Exportação/envio | Cards selecionados, destino, data, resultado e identificadores externos quando existirem. |

Para preservar o texto exato, a implementação deve definir uma representação de posições compatível entre backend e frontend, incluindo Unicode. Tempos por segmento bastam para legenda clicável; destaque da palavra atualmente falada exigiria alinhamento temporal por palavra e é outra decisão.

## 10. Infraestrutura observada e perguntas para o código

| Evidência no Compose | O que permite afirmar | O que precisa ser verificado |
| --- | --- | --- |
| AuthService e PostgreSQL de autenticação | Existe configuração de serviço e banco para autenticação. | Endpoints, fluxo de identidade, autorização e isolamento efetivamente implementados. |
| VideoService e PostgreSQL de vídeos | Existe configuração de serviço e banco de vídeo. | Upload, metadados, estados, transcrições, traduções e cards disponíveis. |
| VideoProcessorWorker | Existe configuração de worker .NET. | Etapas reais, consumidor, ferramentas de mídia e provedores. |
| BFF | Existe configuração de serviço de fachada. | Responsabilidade, contratos com frontend e possíveis chamadas redundantes. |
| MinIO com configurações de notificação Kafka | Há intenção de armazenamento de objetos e publicação de eventos. | Bucket, regra de notificação aplicada e garantia de que o upload dispara evento consumível. |
| Kafka em nó único e Kafka UI | Há configuração de mensageria local e inspeção. | Contratos, grupos de consumo, confirmação, retentativas e recuperação. |
| VideoService e worker recebem a mesma conexão de banco | A conexão configurada aponta para o mesmo banco. | Quem controla schema/migrações e quais operações cada processo executa. |
| Aplicações em Development | O ambiente do Compose está configurado para desenvolvimento. | Configuração específica de produção; o arquivo não demonstra uma implantação de produção. |

Pontos concretos para a análise posterior: prontidão de dependências além de `depends_on`; credenciais MinIO literais no arquivo; imagens com `latest`; publicação de portas; correspondência entre portas configuradas e as portas realmente escutadas; consistência entre objeto, registro e evento. Não concluir falhas de implementação sem ler o código/configuração complementar.

## 11. Cenários essenciais de aceitação

| Cenário | Resultado esperado |
| --- | --- |
| Vídeo válido com fala e idioma suportado | Upload → processamento → transcrição → reprodução com legenda sincronizada. |
| Buscar outro ponto e alterar velocidade | A legenda passa a acompanhar o novo tempo sem acumular atraso. |
| Frase dividida entre duas legendas | Clique consulta a frase completa reconstruída, não apenas a legenda visível. |
| Mesma palavra com dois significados | Cada ocorrência recebe tradução adequada ao próprio contexto. |
| Expressão composta | A consulta mostra o significado da expressão e preserva a seleção original. |
| Clicar em A e depois B; resposta A chega por último | O painel continua mostrando a consulta B. |
| Trocar idioma de destino durante consulta | A resposta anterior não é exibida no idioma novo. |
| Salvar e exportar caracteres acentuados e não latinos | Conteúdo permanece correto no Anki. |
| Card com aspas, tabulações ou quebras de linha | Importação preserva quantidade e mapeamento de campos. |
| Anki indisponível ou envio incerto | Card é preservado; alternativa de arquivo permanece utilizável. |
| Worker reinicia ou recebe evento duplicado | Trabalho recupera ou falha claramente, sem duplicidade indevida. |
| Vídeo excluído durante processamento | Resultado tardio não republica o vídeo. |
| Reprocessar transcrição | Novas consultas usam a versão nova; cards antigos preservam o conteúdo revisado. |
| Usuário tenta acessar vídeo alheio | Requisição e acesso à mídia são recusados. |

## 12. Ordem recomendada de implementação

1. **Mapear a base atual:** confirmar recursos existentes e lacunas; evitar reimplementar o que já funciona.
2. **Concluir upload → processamento → resultado persistido:** incluindo falha, retentativa e acesso autorizado ao vídeo.
3. **Entregar o player e a legenda original clicável:** validar tempos, busca e velocidade com vídeos reais.
4. **Entregar contexto e tradução:** selecionar ocorrência, reconstruir frase e mostrar significado contextual.
5. **Entregar card, lista e exportação:** testar importação real no Anki antes de declarar o MVP concluído.
6. **Validar integração direta:** incorporar se viável; manter o arquivo como alternativa.
7. **Investigar YouTube:** primeiro aquisição de dados e reprodução, depois reutilização do fluxo.

O MVP está completo quando o usuário consegue transformar uma palavra encontrada em um vídeo enviado em um card efetivamente importável no Anki.

## 13. Decisões ainda abertas

| Decisão | Proposta inicial / investigação necessária |
| --- | --- |
| Idiomas do MVP | Escolher um conjunto pequeno após verificar cobertura dos provedores; não prometer suporte universal. |
| Provedores de transcrição e tradução | Avaliar qualidade, timestamps, latência, custo e necessidade de enviar mídia/texto a terceiros. |
| Formatos, duração e tamanho | Definir após ensaio com infraestrutura e codecs usados pelo player. |
| Tipo de aplicação e frontend | Não identificável apenas pelo Compose. Confirmar no repositório. |
| Idioma misto | Um idioma predominante no MVP; detalhamento por trecho depois. |
| Lista momentânea | Recomenda-se persistência por usuário até remoção manual. |
| Retenção de vídeos e arquivos auxiliares | Definir duração e política de limpeza; cards textuais sobrevivem à exclusão do vídeo. |
| Anki direto | Fazer prova técnica no navegador/desktop alvo antes de assumir disponibilidade. |
| YouTube | Investigar separadamente obtenção de áudio/legendas; player incorporado é apenas parte da solução. |

## 14. Referências técnicas consultadas

- [Manual do Anki — importação de arquivos de texto](https://docs.ankiweb.net/manual/importing/text-files): UTF-8, separadores, HTML, mapeamento de campos e tratamento de duplicatas.
- [AnkiConnect — página do complemento no AnkiWeb](https://ankiweb.net/shared/info/2055492159): candidato à integração; requer prova técnica antes de fechar o requisito de envio direto.
- [YouTube IFrame Player API](https://developers.google.com/youtube/iframe_api_reference): reprodução incorporada, eventos e controle do tempo do player.

Referências verificadas em 08/10/2026. Os requisitos e metas propostos neste documento são decisões de produto, não garantias fornecidas por essas ferramentas.
