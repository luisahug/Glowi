# Regras de negócio do Glowi

Este documento descreve as regras que o Glowi segue para contas, serviços, horários e agendamentos.

Todos os horários do sistema seguem o **horário de Brasília**.

## 1. Contas e perfis

- No cadastro, cada pessoa escolhe se é **Cliente** ou **Profissional**. O perfil não muda depois.
- O email é único e serve de login.
- A senha precisa ter no mínimo 6 caracteres, com letra maiúscula, letra minúscula, número e símbolo.
- A profissional precisa informar sua **especialidade**. Telefone e biografia são opcionais.
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
- Agendamentos já marcados no período **não são cancelados automaticamente**. O sistema avisa a profissional de quantos existem, e ela decide se recusa os pendentes ou combina com a cliente.
- O motivo do bloqueio é opcional (até 120 caracteres) e **só a profissional vê**.
- Quando a profissional remove um bloqueio, os horários voltam a ficar livres. Bloqueios que já terminaram saem da lista.
