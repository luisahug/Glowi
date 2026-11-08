# Regras de negócio do Glowi

Este documento descreve as regras que o Glowi segue para contas, serviços, horários e agendamentos.

Todos os horários do sistema seguem o **horário de Brasília**.

## 1. Contas e perfis

- No cadastro, cada pessoa escolhe se é **Cliente** ou **Profissional**. O perfil não muda depois.
- O email é único e serve de login.
- A senha precisa ter no mínimo 6 caracteres, com letra maiúscula, letra minúscula, número e símbolo.
- A profissional precisa informar sua **especialidade**. Telefone, biografia e endereço são opcionais (o endereço é recomendado, ver seção 8).
- Uma conta com agendamentos registrados **não pode ser excluída**, para não apagar o histórico da outra parte.

## 2. Serviços

- Cada serviço da profissional tem nome, duração (de **5 a 480 minutos**) e preço (de **R$ 0,01 a R$ 10.000,00**).
- Cada serviço pode ter até **10 fotos** (JPG, PNG ou WEBP, com até **5 MB** cada).
- A profissional só vê e altera os próprios serviços.
- Alterar o preço de um serviço não muda os agendamentos já feitos (ver 5.3).

## 3. Horários de atendimento

- A profissional informa em quais **faixas de horário** atende em cada dia da semana. Exemplo: segunda, das 09:00 às 12:00 e das 13:00 às 18:00.
- O fim da faixa precisa ser depois do início.
- Faixas do mesmo dia não podem se sobrepor.
- Dia sem faixa cadastrada é dia em que a profissional não atende.

## 4. Horários livres para agendar

Um horário aparece como livre para a cliente quando atende a todas estas condições:

1. Está dentro de uma faixa de atendimento da profissional. Os horários são oferecidos de **15 em 15 minutos** a partir do início da faixa.
2. O serviço inteiro **termina dentro da faixa**.
3. Faltam pelo menos **2 horas** para o horário começar.
4. Não se sobrepõe a nenhum agendamento **pendente ou confirmado** da profissional. Um pedido pendente já reserva o horário.
5. Não cai num **bloqueio de agenda** da profissional (ver seção 7).

A cliente pode agendar a partir de hoje e até **30 dias à frente**.

### 4.1 Agendamentos ao mesmo tempo

- O(a) cliente **não pode ter dois agendamentos ao mesmo tempo**, mesmo com profissionais diferentes. Contam os agendamentos **pendentes ou confirmados** que se sobrepõem ao novo horário (considerando a duração do serviço).
- Na lista de horários, os que coincidem com outro agendamento do(a) cliente aparecem **riscados e desabilitados**, com um aviso de qual agendamento está no caminho.
- Na tela de confirmação, se houver conflito, aparece o aviso e o botão Confirmar some. O(a) cliente precisa **cancelar primeiro** o agendamento concomitante em "Meus agendamentos".
- O servidor também recusa o pedido com essa mensagem, caso o(a) cliente chegue ao horário por outro caminho.

## 5. Agendamento

### 5.1 Criação

- Só clientes fazem agendamentos.
- Ao confirmar, o sistema verifica de novo se o horário continua livre. Se outra pessoa reservou antes, a cliente é avisada e escolhe outro horário.
- Nunca existem dois agendamentos ativos no mesmo horário para a mesma profissional, nem quando duas clientes confirmam ao mesmo tempo.

### 5.2 Status

```
           ┌──► Confirmado ──► Concluído (quando o horário termina)
Pendente ──┼──► Recusado
           └──► Cancelado
Confirmado ───► Cancelado
```

| Status | Quem muda | Quando pode mudar |
|---|---|---|
| **Pendente** | — | Todo agendamento começa assim. |
| **Confirmado** | A profissional do agendamento | Enquanto o pedido está pendente e o horário ainda não terminou. |
| **Recusado** | A profissional do agendamento | Nas mesmas condições do Confirmado. |
| **Cancelado** | A cliente do agendamento | Quando está pendente ou confirmado e **ainda não começou**. Não há prazo mínimo para cancelar. |
| **Concluído** | Automático | Quando um agendamento confirmado chega ao horário de término. |

- Um pedido pendente que a profissional não responde até o fim do horário vai para o histórico e não pode mais ser confirmado.
- Quando um agendamento é cancelado ou recusado, o horário fica livre na hora para outras clientes.
- A profissional não cancela um agendamento já confirmado. Se precisar, ela combina com a cliente pelo telefone ou WhatsApp que aparece no agendamento.

### 5.3 Preço

O agendamento guarda o preço do serviço **no momento da reserva**. Esse valor não muda se a profissional alterar o serviço depois. O faturamento previsto da agenda soma só os agendamentos confirmados.

## 6. Favoritas

- Só clientes favoritam profissionais.
- Uma mesma profissional entra só uma vez nas favoritas da cliente.
- A lista "Minhas favoritas" mostra as profissionais em ordem alfabética, com atalho para os serviços de cada uma.
- Se a conta da cliente ou da profissional for excluída, os favoritos ligados a ela somem junto.

## 7. Bloqueios de agenda

- A profissional pode bloquear **dias inteiros** (um ou vários seguidos) ou um **período com hora de início e fim**. Exemplo: 10/11, das 14:00 às 16:00.
- O fim precisa ser depois do início, o período não pode ter terminado e o início pode estar no máximo **1 ano à frente**.
- Bloqueios da mesma profissional não podem se sobrepor.
- Enquanto o bloqueio existe, as clientes não veem horários livres no período.
- Se já houver agendamentos **pendentes ou confirmados** (que ainda não começaram) no período, o bloqueio **não é criado direto**. O sistema mostra a lista desses agendamentos e a profissional escolhe:
  - **Voltar sem bloquear**: nada muda.
  - **Confirmar**: o bloqueio é criado, os pedidos pendentes viram **Recusado** e os confirmados viram **Cancelado**, marcados como **cancelados pela profissional**.
- O cliente é avisado no app: o menu "Meus agendamentos" mostra um contador e, ao abrir a página, aparece um alerta com os agendamentos cancelados. O alerta aparece **uma vez** (ao abrir a página, conta como visto). No histórico, o agendamento fica com o selo "Cancelado pela profissional".
- A tela de confirmação também mostra o WhatsApp de cada cliente, caso a profissional queira falar com ela antes.
- Agendamentos que já começaram não são cancelados pelo bloqueio.
- O motivo do bloqueio é opcional (até 120 caracteres) e **só a profissional vê**.
- Quando a profissional remove um bloqueio, os horários voltam a ficar livres. Bloqueios que já terminaram saem da lista.

## 8. Localização e busca por distância

### 8.1 Endereço da profissional

- A profissional pode informar o **endereço de atendimento** no cadastro ou depois, no perfil. É **opcional, mas recomendado**.
- Ao digitar o CEP, rua, bairro, cidade e UF são preenchidos automaticamente (ViaCEP). Ela confere e informa o número.
- Também pode usar o botão **"Usar minha localização atual"**, que pega a posição do GPS do aparelho e preenche o endereço.
- O sistema converte o endereço em coordenadas (OpenStreetMap). Se não encontrar o endereço no mapa, salva assim mesmo e avisa no perfil para conferir o CEP e o número ou usar a localização atual.
- As clientes veem **só o bairro e a cidade** da profissional, nunca o endereço completo.

### 8.2 Localização da cliente

- Na primeira vez que abre a busca, o navegador pede permissão para usar a localização. Se ela aceitar, a busca já passa a considerar onde ela está.
- A cliente pode trocar o local quando quiser: pelo GPS ou digitando um CEP ou "bairro, cidade".
- O raio padrão é de **15 km**. Ela pode escolher 5, 10, 15, 25, 50 ou 100 km.
- O local e o raio ficam salvos **no navegador** (cookie), valem também sem login e só naquele aparelho.
- Ela pode desligar o filtro com "Não filtrar por distância". Aí a busca volta a mostrar todas as profissionais.

### 8.3 Resultado da busca

- Com a localização definida, aparecem só as profissionais **dentro do raio**, da mais perto para a mais longe, com a distância em cada card.
- Profissionais que **não informaram endereço** continuam aparecendo, **no fim da lista**, marcadas como "distância não informada".
- A distância é em linha reta (não é o trajeto de carro ou a pé).
- Os outros filtros (tipo de profissional, texto, data e turno) continuam valendo junto com a distância.
