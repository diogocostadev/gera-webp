# gerawebp

## Visão Geral
`gerawebp` é uma aplicação web ASP.NET MVC desenvolvida pela Código Central. Esta ferramenta permite a conversão de arquivos de imagem para o formato .webp, visando otimizar o processo de manipulação de imagens dentro dos fluxos de trabalho internos da empresa.

## Funcionalidades Principais
- **Conversão Otimizada**: Converte imagens JPEG, PNG e GIF para WebP com compressão inteligente
- **Preservação de Dimensões**: Mantém as dimensões originais das imagens enviadas
- **Perfis de Otimização**: Aplica diferentes níveis de compressão baseados no tamanho do arquivo original
- **Interface Intuitiva**: Interface web simples para upload e conversão em lote
- **Progresso em Tempo Real**: Acompanhe o progresso da conversão via SignalR

## Melhorias de Otimização Implementadas

### Configurações Avançadas de Compressão
- **WebpEncodingMethod.BestQuality**: Utiliza o melhor algoritmo de compressão
- **FilterStrength**: Filtro de deblocking adaptativo (40-80 baseado no tamanho)
- **SpatialNoiseShaping**: Redução de ruído espacial (30-70)
- **UseSharpYuv**: Melhora a qualidade em bordas e detalhes
- **AlphaQuality**: Otimização do canal alpha para transparência

### Perfis de Otimização Automática
- **Arquivos < 1MB**: Compressão suave (qualidade original)
- **Arquivos 1-5MB**: Compressão balanceada (qualidade -5)
- **Arquivos 5-10MB**: Compressão alta (qualidade -10)
- **Arquivos > 10MB**: Compressão máxima (qualidade -15)

### Processamento Sem Redimensionamento
- **Dimensões Originais**: Mantém as dimensões exatas da imagem original
- **Qualidade Preservada**: Foco na compressão sem alteração de resolução
- **Sharpening Adaptativo**: Aplicado em imagens grandes para compensar compressão

## Requisitos
- .NET Core 8 ou superior
- Visual Studio 2019 ou superior

## Configuração e Instalação
Clone o repositório do `gerawebp` para sua máquina local usando:

```bash
git clone https://gitlab.codigocentral.com.br/codigocentral/gerawebp.git
cd gerawebp
```

Abra a solução no Visual Studio e restaure os pacotes NuGet:

```bash
dotnet restore
```

Compile a solução:

```bash
dotnet build
```

## Execução
Para rodar o `gerawebp` localmente, execute:

```bash
dotnet run
```

Navegue até `http://localhost:5000` para acessar a aplicação.

## Como Usar
1. **Upload**: Selecione uma ou mais imagens (JPEG, PNG, GIF)
2. **Configuração**: Ajuste apenas a qualidade (0-100%)
3. **Conversão**: Clique em "Gerar WebP" e acompanhe o progresso
4. **Download**: Baixe o arquivo ZIP com todas as imagens convertidas

## Dicas para Melhor Compressão
- **Qualidade 60-80%**: Oferece o melhor equilíbrio entre tamanho e qualidade
- **Formato Original**: JPEGs geralmente comprimem melhor que PNGs para fotos
- **Transparência**: PNGs com transparência mantêm a qualidade do canal alpha
- **Dimensões**: As imagens mantêm suas dimensões originais para preservar detalhes

## Desenvolvimento
`gerawebp` é um projeto de código fechado, desenvolvido exclusivamente para uso interno da Código Central. Não são aceitas contribuições externas.

## Suporte
Para obter suporte, entre em contato com o departamento de TI da Código Central.

## Autores
Código Central

## Licença
Este software é de propriedade exclusiva da Código Central e seu uso é estritamente limitado a aplicações internas e comerciais da empresa. Para mais detalhes, consulte o arquivo `LICENSE.md`.



teste traefik feature/teste-traefik
teste 2
teste 3
teste 4
teste 5