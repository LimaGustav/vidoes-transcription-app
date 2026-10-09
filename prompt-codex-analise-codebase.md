# Prompt para o Codex analisar a base atual

Copie o texto abaixo para o Codex aberto na raiz do projeto. Disponibilize também `requisitos-plataforma-idiomas.md` e o Docker Compose, se ainda não estiverem no repositório.

---

Quero que você analise minha base de código atual e me ajude a decidir os próximos passos de desenvolvimento de uma plataforma de aprendizado de idiomas com vídeos.

## Contexto do produto

O usuário envia um vídeo. O sistema detecta o idioma, gera a transcrição original com timestamps e reproduz o vídeo com legendas sincronizadas. Ao clicar em uma palavra, deve recuperar a frase completa em que aquela ocorrência aparece, mostrar a tradução da frase para o idioma escolhido e explicar o significado da palavra naquele contexto, considerando expressões de várias palavras quando necessário.

Após consultar a tradução, o usuário pode revisar um card e enviá-lo ao Anki. Se a integração direta estiver indisponível, pode adicionar à lista e exportar um arquivo de texto UTF-8 compatível com importação no Anki. Exportação em lote deve funcionar sem integração direta. Posteriormente quero aceitar links do YouTube, sem exigir upload manual.

Leia `requisitos-plataforma-idiomas.md`, se disponível, e use-o como referência para o comportamento esperado. Se não o encontrar, faça a análise com o contexto deste prompt e informe a limitação.

## Objetivo desta primeira etapa

**Faça diagnóstico e propostas, sem alterar código, arquivos, configuração, dependências ou dados.** Não aplique refatorações e não implemente funcionalidades nesta etapa. Quero entender a situação antes de aprovar mudanças.

Leia as instruções do repositório e preserve alterações locais existentes. Use inspeção estática primeiro. Pode executar verificações que não iniciem infraestrutura, não acessem serviços externos e não modifiquem dados; informe quais executou e suas limitações. Se um build/teste exigir restauração, criação de artefatos ou acesso externo, apresente o comando e peça autorização antes de executar. Não rode migrações nem suba o Compose nesta etapa.

Não exponha segredos encontrados em arquivos, logs ou configuração. Informe somente onde estão, de que tipo são e como corrigir o armazenamento.

## 1. Mapeie o que existe

- Localize projetos, solução, frontend, serviços, workers, bibliotecas compartilhadas, testes e configuração.
- Identifique versões, responsabilidades e dependências reais a partir dos arquivos.
- Descreva a comunicação entre frontend/BFF, AuthService, VideoService, VideoProcessorWorker, PostgreSQL, MinIO e Kafka, confirmando quais componentes efetivamente existem no código.
- Rastreie o fluxo de upload e processamento desde o ponto de entrada até os resultados persistidos: endpoint → registro → armazenamento → evento → consumidor → processamento → estado retornado à interface.
- Procure recursos já implementados para transcrição, idioma, timestamps, reprodução, contexto, tradução, cards e exportação.
- Classifique cada recurso como **implementado**, **parcial**, **ausente** ou **não foi possível verificar**. Comente a qualidade quando houver implementação.

Para cada conclusão relevante, cite o caminho do arquivo e o símbolo/trecho que a sustenta. Não trate nomes de classes, comentários, TODOs ou configurações do Compose como prova de comportamento completo.

## 2. Analise a arquitetura e o código

Avalie o que ajudar concretamente o produto:

- Clareza das responsabilidades e organização; duplicação, acoplamento e abstrações que complicam o fluxo.
- Se BFF, serviços e worker têm responsabilidades justificadas. Considere a simplicidade necessária para o MVP sem prescrever reescrita ou novos microserviços.
- Contratos de API/eventos, validação, autorização por proprietário, erros, async/await, cancelamento e timeouts.
- Acesso a dados, transações, concorrência, consultas redundantes e origem das migrações.
- O fato de VideoService e worker receberem a conexão do mesmo banco: quem controla schema, quem escreve o quê e como se mantém consistência.
- Isolamento dos provedores de transcrição/tradução e dados necessários para trocar de provedor sem espalhar alterações.
- Testes existentes e lacunas que possam esconder falhas no fluxo principal.

Sugira refatorações pequenas ligadas a problemas observados. Não recomende padrões, camadas ou testes apenas por preferência estética.

## 3. Verifique processamento, mensageria e armazenamento

Investigue:

- Se o evento de upload realmente é publicado: variáveis de notificação do MinIO não demonstram sozinhas que bucket e regra de evento estão configurados.
- Se a ordem entre objeto persistido, metadados e publicação pode produzir arquivo órfão, evento sem registro ou vídeo preso em fila.
- Identidade do evento/trabalho, tratamento de eventos repetidos, concorrência e confirmação de consumo em relação à persistência do resultado.
- Comportamento em reinício do worker, indisponibilidade das dependências e falha parcial.
- Retentativas limitadas, espera, tratamento de falha definitiva e reaproveitamento de etapas concluídas.
- Se vídeo excluído ou reprocessado pode receber resultado antigo por engano.
- Limpeza de arquivos temporários, acesso autorizado à mídia e reprodução com busca temporal.

Descreva riscos concretos e proponha a solução proporcional ao caso. Se considerar outbox, fila de falhas ou outra técnica, explique qual falha existente ela resolveria e seu custo; não a imponha automaticamente.

## 4. Examine o Docker Compose junto com o código

O Compose recebido configura serviços .NET em Development, dois PostgreSQL, MinIO, Kafka em nó único, Kafka UI e BFF.

Verifique:

- Valores esperados no `.env`, arquivos de exemplo e configuração efetivamente consumida pelas aplicações.
- Endereços internos versus externos, listeners do Kafka e portas realmente escutadas. Mapear uma variável de porta não configura automaticamente a porta do processo dentro do container.
- Prontidão e reconexão: diferencie ordem de inicialização por `depends_on` de dependência saudável e pronta.
- Healthchecks, volumes, inicialização de buckets/tópicos e execução de migrações.
- Credenciais literais, portas publicadas, imagens sem versão fixa e separação entre configuração local e de produção.
- Se os Dockerfiles e arquivos de configuração referenciados existem e são coerentes.

Não conclua que uma configuração local prova um problema em produção. Diferencie **problema comprovado**, **risco a validar** e **escolha aceitável para desenvolvimento**.

## 5. Compare a base com os requisitos

Monte uma matriz com: requisito/funcionalidade, estado atual, evidência, lacuna e próximo passo.

Preste atenção a estes detalhes:

- Uma legenda pode ser apenas parte de uma frase; o clique precisa recuperar a frase completa.
- A mesma palavra repetida exige identificar a ocorrência correta.
- Tempos por segmento podem bastar para legenda clicável; não exigir tempos por palavra se não houver destaque da palavra falada.
- Tokenização e posições de texto precisam preservar Unicode e corresponder entre frontend e backend.
- Tradução da palavra depende do contexto e pode corresponder a uma expressão.
- Respostas fora de ordem e mudança de idioma não podem mostrar tradução antiga.
- Cache precisa distinguir contexto, ocorrência, idiomas e versão da transcrição.
- Cards preservam texto revisado mesmo após reprocessamento/exclusão do vídeo.
- Arquivos devem ser importáveis no Anki, inclusive com caracteres não latinos, aspas e quebras de linha.
- Backend hospedado não deve ser presumido capaz de acessar o Anki local do usuário; proponha uma prova técnica para integração direta.
- O player do YouTube e a aquisição de áudio/transcrição são problemas separados; mantenha YouTube fora da entrega inicial.

## 6. Priorize os próximos passos

Proponha uma sequência curta que complete primeiro:

**upload → processamento → transcrição persistida → player sincronizado → tradução contextual → card → exportação/importação no Anki**.

Adapte a sequência ao que já está implementado. Não reinicie o projeto nem refaça componentes que já cumprem o objetivo.

Para cada passo, informe:

1. Problema/lacuna que resolve e benefício para o usuário.
2. Arquivos/componentes provavelmente envolvidos.
3. Dependências e ordem em relação aos outros passos.
4. Esforço relativo — pequeno, médio ou grande — e principais incertezas.
5. Risco e forma prática de validar.
6. Critério observável para considerar concluído.

Separe correções que bloqueiam o fluxo, melhorias recomendadas e evoluções opcionais. Indique o primeiro incremento pequeno que vale executar após minha aprovação.

## Formato da resposta

Entregue:

1. Diagnóstico breve: o que funciona, principal lacuna e recomendação inicial.
2. Mapa da arquitetura atual e rastreamento do fluxo principal; use diagrama apenas se ajudar.
3. Matriz de funcionalidades/requisitos.
4. Achados priorizados com evidência, impacto, solução sugerida e esforço. Distingua fatos e hipóteses.
5. Plano de próximos passos, com critérios de conclusão.
6. Proposta concreta do primeiro incremento: escopo, arquivos, mudanças e validação para eu aprovar.
7. Verificações executadas e limitações da análise.

Se não houver evidência suficiente, diga exatamente o que falta verificar. Faça perguntas somente quando a resposta mudar uma decisão importante e não puder ser obtida no repositório.

**Pare depois da análise e da proposta. Nas etapas seguintes, apresente o que pretende alterar e peça minha permissão antes de cada conjunto de mudanças. Uma aprovação vale apenas para o escopo apresentado; ao surgir uma alteração nova, explique-a antes de executar.**
